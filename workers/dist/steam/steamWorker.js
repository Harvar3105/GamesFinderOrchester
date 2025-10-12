import amqp from 'amqplib';
import { scrapeBatch } from '../utils/types/fetch/fetchAPI';
import { config, redis } from '../utils/config';
async function startSteamWorker() {
    const connection = await amqp.connect(config.rabbitUrl);
    const channel = await connection.createChannel();
    await channel.assertQueue(config.steamQueue, { durable: true });
    console.log(`Steam worker listening on queue "${config.steamQueue}"...`);
    channel.consume(config.steamQueue, async (msg) => {
        if (!msg)
            return;
        const task = JSON.parse(msg.content.toString());
        console.log('Received task:', task.jobId, 'IDs:', task.ids.length);
        if (await redis.exists(task.redisResultKey)) {
            await redis.del(task.redisResultKey);
            console.log(`Cleared existing Redis key from previous request: ${task.redisResultKey}`);
        }
        var batches = splitIntoBatches(task.ids, config.maxRequests);
        try {
            let scrapedCount = 0;
            for (const batch of batches) {
                const result = await scrapeBatch(batch);
                if (result.length > 0) {
                    scrapedCount += result.length;
                    await redis.rpush(task.redisResultKey, ...result.map(r => JSON.stringify(r)));
                    console.log(`Just added ${result.length} games to redis, total so far: ${scrapedCount}`);
                }
                await new Promise(res => setTimeout(res, config.cooldownMs));
            }
            channel.sendToQueue(config.steamResultsQueue, Buffer.from(JSON.stringify({
                jobId: task.jobId,
                redisResultKey: task.redisResultKey,
            })));
            channel.ack(msg);
            console.log(`Task ${task.jobId} done, scraped ${scrapedCount} games.`);
        }
        catch (err) {
            console.error('Error processing task:', err);
            channel.nack(msg, false, true);
        }
    });
}
function splitIntoBatches(array, batchSize) {
    const batches = [];
    for (let i = 0; i < array.length; i += batchSize) {
        batches.push(array.slice(i, i + batchSize));
    }
    return batches;
}
startSteamWorker().catch(console.error);
