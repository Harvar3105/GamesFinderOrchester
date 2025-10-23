import { describe, it, beforeEach, expect, jest } from '@jest/globals';

jest.unstable_mockModule('../src/utils/config.js', () => ({
  rabbitConn: {
    createChannel: jest.fn(),
  },
  config: {
    steamRequests: 'steam.requests',
    steamResults: 'steam.results',
    maxRequests: 200,
    cooldownMs: 0,
  },
  redis: {
    exists: jest.fn(),
    del: jest.fn(),
    rpush: jest.fn(),
  },
}));

jest.unstable_mockModule('../src/utils/fetchAPI.js', () => ({
  scrapeBatch: jest.fn(),
}));

jest.unstable_mockModule('../src/utils/logger.js', () => ({
  default: {
    info: jest.fn(),
    warn: jest.fn(),
    error: jest.fn(),
    crit: jest.fn(),
  },
}));

const { startSteamWorker } = await import('../src/steam/steamWorker.js');
const { rabbitConn, redis, config } = await import('../src/utils/config.js');
const { scrapeBatch } = await import('../src/utils/fetchAPI.js');
const logger = (await import('../src/utils/logger.js')).default;

describe('Steam Worker', () => {
  let fakeChannel: any;

  beforeEach(() => {
    jest.clearAllMocks();

    fakeChannel = {
      assertQueue: jest.fn(),
      consume: jest.fn(),
      sendToQueue: jest.fn(),
      ack: jest.fn(),
      nack: jest.fn(),
    };

    (rabbitConn.createChannel as jest.Mock).mockResolvedValue(fakeChannel);
  });

  it('должен обработать задачу и записать результаты в Redis', async () => {
    const fakeMsg = {
      content: Buffer.from(
        JSON.stringify({
          taskId: '123',
          gameIds: [1, 2],
          redisResultKey: 'steam:123',
        })
      ),
    };

    (redis.exists as jest.Mock).mockResolvedValue(false);
    (scrapeBatch as jest.Mock).mockResolvedValue([
      { id: 'g1', name: 'Half-Life' },
      { id: 'g2', name: 'Portal' },
    ]);

    (fakeChannel.consume as jest.Mock).mockImplementation(
      async (_queue: string, callback: Function) => {
        await callback(fakeMsg);
      }
    );

    await startSteamWorker();

    expect(fakeChannel.assertQueue).toHaveBeenCalledWith(config.steamRequests, { durable: true });
    expect(fakeChannel.assertQueue).toHaveBeenCalledWith(config.steamResults, { durable: true });

    expect(scrapeBatch).toHaveBeenCalledTimes(1);
    expect(redis.rpush).toHaveBeenCalledWith(
      'steam:123',
      expect.stringContaining('Half-Life'),
      expect.stringContaining('Portal')
    );
    expect(fakeChannel.sendToQueue).toHaveBeenCalledWith(
      config.steamResults,
      expect.any(Buffer),
      { persistent: true }
    );
    expect(fakeChannel.ack).toHaveBeenCalledWith(fakeMsg);
  });

  it('должен корректно обработать невалидный JSON', async () => {
    const fakeMsg = { content: Buffer.from('invalid_json') };

    (fakeChannel.consume as jest.Mock).mockImplementation(
      async (_queue: string, callback: Function) => {
        await callback(fakeMsg);
      }
    );

    await startSteamWorker();

    expect(fakeChannel.nack).toHaveBeenCalledWith(fakeMsg, false, false);
    expect(logger.error).toHaveBeenCalledWith(
      '❌Invalid JSON from queue:',
      'invalid_json'
    );
  });
});
