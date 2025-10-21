export interface Task {
  taskId: string;
  createdAt: string;
}

export interface SteamTask extends Task {
  gameIds: number[];
  updateExisting: boolean;
  redisResultKey: string;
}

export function normalizeSteamTask(raw: any): SteamTask {
  return {
    taskId: raw.TaskId ?? raw.taskId,
    gameIds: raw.GameIds ?? raw.gameIds,
    updateExisting: raw.UpdateExisting ?? raw.updateExisting,
    redisResultKey: raw.RedisResultKey ?? raw.redisResultKey,
    createdAt: raw.CreatedAt ?? raw.createdAt
  };
}