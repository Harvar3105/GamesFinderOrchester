export interface Task {
  jobId: string;
  createdAt: string;
}

export interface SteamTask extends Task {
  ids: number[];
  updateExisting: boolean;
  redisResultKey: string;
}
