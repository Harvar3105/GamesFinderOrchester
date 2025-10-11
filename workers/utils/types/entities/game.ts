import { ECurrency } from "../enums/eCurrency";
import { Entity } from "./entity";
import { GameOffer } from "./gameOffer";

export interface Game extends Entity {
  name: string;
  steamUrl: string;
  steamID: number;
  inPackages: number[];
  isDLC: boolean;
  description: string | null;
  headerImage: string | null;
  offers: GameOffer[];
  isReleased: boolean;
  initialPrices: Record<ECurrency, number>;
}