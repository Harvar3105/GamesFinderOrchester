import { eCurrency } from "../enums/eCurrency.js";
import { eVendor } from "../enums/eVendor.js";
import { Entity } from "./entity.js";


export interface GameOffer extends Entity {
  gameId: string;
  vendorsGameId: string;
  vendor: eVendor;
  vendorsUrl: string;
  available: boolean;
  price: Record<eCurrency, number> | null;
}