import { rabbitConn } from '../utils/config.js';
import { config, redis } from '../utils/config.js';
import { SteamTask } from '../utils/types/entities/tasks.js';
import { scrapeBatch } from '../utils/fetchAPI.js';
import { Game } from '../utils/types/entities/game.js';

async function startSteamWorker() {
  const channel = await rabbitConn.createChannel();

  await channel.assertQueue(config.steamRequests!, { durable: true });
  console.log(`Steam worker listening on queue "${config.steamRequests}"...`);

  await channel.assertQueue(config.steamResults!, { durable: true });
  console.log(`Steam worker ready to answear on queue "${config.steamResults}"...`)

  channel.consume(config.steamRequests!, async (msg) => {
    if (!msg) return;

    const task: SteamTask = JSON.parse(msg.content.toString());
    console.log('Received task:', task.jobId, 'IDs:', task.ids.length);

    if (await redis.exists(task.redisResultKey)) {
      await redis.del(task.redisResultKey);
      console.log(`Cleared existing Redis key from previous request: ${task.redisResultKey}`);
    }
    
    var batches = splitIntoBatches(task.ids, config.maxRequests);

    try {
      let scrapedCount = 0;
      for (const batch of batches) {
        const result: Game[] = await scrapeBatch(batch);
        
        if (result.length > 0) {
          scrapedCount += result.length;

          await redis.rpush(task.redisResultKey, ...result.map(r => JSON.stringify(r)));
          console.log(`Just added ${result.length} games to redis, total so far: ${scrapedCount}`);
        }

        await new Promise(res => setTimeout(res, config.cooldownMs));
      }

      await channel.sendToQueue(
        config.steamResults!, 
        Buffer.from(JSON.stringify({
          jobId: task.jobId,
          redisResultKey: task.redisResultKey
        })),
        { persistent: true }
      )
      channel.ack(msg);
      console.log(`Task ${task.jobId} done, scraped ${scrapedCount} games.`);
    } catch (err) {
      console.error('Error processing task:', err);
      channel.nack(msg, false, true);
    }
  });
}

function splitIntoBatches<T>(array: T[], batchSize: number): T[][] {
  const batches: T[][] = [];
  for (let i = 0; i < array.length; i += batchSize) {
    batches.push(array.slice(i, i + batchSize));
  }
  return batches;
}



startSteamWorker().catch(console.error);
