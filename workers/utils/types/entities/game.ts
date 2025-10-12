import { eCurrency } from "../enums/eCurrency";
import { Entity } from "./entity";
import { GameOffer } from "./gameOffer";

export interface Game extends Entity {
  name: string;
  steamUrl: string;
  steamID: number;
  inPackages: number[] | null;
  isDLC: boolean;
  description: string | null;
  headerImage: string | null;
  offers: GameOffer[] | null;
  isReleased: boolean;
  initialPrices: Record<eCurrency, number> | null;
}