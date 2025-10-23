import { jest } from '@jest/globals';

jest.unstable_mockModule('../src/utils/config.js', () => ({
  rabbitConn: { createChannel: jest.fn() },
  config: { 
    steamRequests: 'steam_requests',
    steamResults: 'steam_results',
    maxRequests: 2,
    cooldownMs: 0
  },
  redis: {
    exists: jest.fn(),
    del: jest.fn(),
    rpush: jest.fn()
  }
}));

jest.unstable_mockModule('../src/utils/fetchAPI.js', () => ({
  scrapeBatch: jest.fn()
}));

jest.unstable_mockModule('../src/utils/logger.js', () => ({
  default: {
    info: jest.fn(),
    warn: jest.fn(),
    error: jest.fn(),
    crit: jest.fn()
  }
}));

const { startSteamWorker } = await import('../src/workers/steamWorker.js');
const { rabbitConn, redis } = await import('../src/utils/config.js');
const { scrapeBatch } = await import('../src/utils/fetchAPI.js');

describe('Steam Worker', () => {
  let fakeChannel: any;

  beforeEach(() => {
    jest.clearAllMocks();

    fakeChannel = {
      assertQueue: jest.fn(),
      consume: jest.fn(),
      sendToQueue: jest.fn(),
      ack: jest.fn(),
      nack: jest.fn()
    };

    (rabbitConn.createChannel as jest.Mock).mockResolvedValue(fakeChannel);
  });

  it('Process request and place into Redis', async () => {
    const fakeMsg = {
      content: Buffer.from(JSON.stringify({
        taskId: '123',
        gameIds: [1, 2],
        redisResultKey: 'steam:123'
      }))
    };

    (redis.exists as jest.Mock).mockResolvedValue(false);
    (scrapeBatch as jest.Mock).mockResolvedValue([
      { id: 'g1', name: 'Half-Life' },
      { id: 'g2', name: 'Portal' }
    ]);

    (fakeChannel.consume as jest.Mock).mockImplementation(async (queue, callback) => {
      await callback(fakeMsg);
    });

    await startSteamWorker();

    expect(fakeChannel.assertQueue).toHaveBeenCalledWith('steam_requests', { durable: true });
    expect(scrapeBatch).toHaveBeenCalledTimes(1);
    expect(redis.rpush).toHaveBeenCalledWith(
      'steam:123',
      expect.stringContaining('Half-Life'),
      expect.stringContaining('Portal')
    );
    expect(fakeChannel.sendToQueue).toHaveBeenCalledWith(
      'steam_results',
      expect.any(Buffer),
      { persistent: true }
    );
    expect(fakeChannel.ack).toHaveBeenCalledWith(fakeMsg);
  });

  it('rejects invalid message (invalid JSON)', async () => {
    const fakeMsg = { content: Buffer.from('invalid_json') };
    (fakeChannel.consume as jest.Mock).mockImplementation(async (queue, callback) => {
      await callback(fakeMsg);
    });

    await startSteamWorker();

    expect(fakeChannel.nack).toHaveBeenCalledWith(fakeMsg, false, false);
  });
});
