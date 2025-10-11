import { ECurrency } from "../enums/eCurrency";
import { eVendor } from "../enums/eVendor";
import { Entity } from "./entity";

export interface GameOffer extends Entity {
  gameId: string;
  vendorsGameId: string;
  vendor: eVendor;
  vendorsUrl: string;
  available: boolean;
  price: Record<ECurrency, number>;
}