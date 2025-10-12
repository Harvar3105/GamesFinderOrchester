import { Redis } from 'ioredis';
import dotenv from 'dotenv';
dotenv.config();

export const config = {
  rabbitUrl: process.env.RABBIT_URL || 'amqp://localhost',
  steamQueue: process.env.STEAM_QUEUE || 'steam-scraper',
  steamResultsQueue: process.env.STEAM_RESULTS_QUEUE || 'steam-results',
  maxRequests: Number(process.env.MAX_REQUESTS) || 200,
  cooldownMs: Number(process.env.COOLDOWN_MS) || 5 * 60 * 1000
};

export const redis = new Redis(process.env.REDIS_URL || 'redis://localhost:6379');