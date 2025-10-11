import { Entity } from "./entity";
import { PriceRange } from "./priceRange";

export interface Game extends Entity {
  name: string;
  steamUrl: string;
  steamID: number;
  inPackages: number[];
  isDLC: boolean;
  description: string | null;
  headerImage: string | null;
  // Offers not implemented here. No reason.
  price: PriceRange;
}