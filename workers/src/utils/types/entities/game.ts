import { eCurrency } from "../enums/eCurrency.js";
import { Entity } from "./entity.js";
import { GameOffer } from "./gameOffer.js";

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