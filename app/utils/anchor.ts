import { Program, IdlAccounts, BN, AnchorProvider } from "@coral-xyz/anchor";
import IDL from "../idl/lumberjack.json";
import { Lumberjack } from "@/idl/lumberjack";
import { PublicKey } from "@solana/web3.js";
import { WrappedConnection } from "./wrappedConnection";
import {
  AnchorWallet,
  useConnection,
  useWallet,
} from "@solana/wallet-adapter-react";

export const CONNECTION = new WrappedConnection(
  process.env.NEXT_PUBLIC_RPC
    ? process.env.NEXT_PUBLIC_RPC
    : "https://rpc.magicblock.app/devnet",
  {
    wsEndpoint: process.env.NEXT_PUBLIC_WSS_RPC
      ? process.env.NEXT_PUBLIC_WSS_RPC
      : "wss://rpc.magicblock.app/devnet",
    commitment: "confirmed",
  }
);

export const METAPLEX_READAPI =
  "https://devnet.helius-rpc.com/?api-key=78065db3-87fb-431c-8d43-fcd190212125";

// Here you can basically use what ever seed you want. For example one per level or city or whatever.
export const GAME_DATA_SEED = "level_2";

export const PROGRAM_ID = new PublicKey(IDL.address);

export function getProgram(provider: AnchorProvider) {
  return new Program(IDL as Lumberjack, provider);
}

export function useAnchorProvider() {
  const { connection } = useConnection();
  const wallet = useWallet();

  return new AnchorProvider(connection, wallet as AnchorWallet, {
    commitment: "confirmed",
  });
}

export const [gameDataPDA] = PublicKey.findProgramAddressSync(
  [Buffer.from(GAME_DATA_SEED, "utf8")],
  PROGRAM_ID
);

// Player Data Account Type from Idl
export type PlayerData = IdlAccounts<Lumberjack>["playerData"];
export type GameData = IdlAccounts<Lumberjack>["gameData"];

// Constants for the game
export const TIME_TO_REFILL_ENERGY: BN = new BN(60);
export const MAX_ENERGY = 100;
export const ENERGY_PER_TICK: BN = new BN(1);
export const TOTAL_WOOD_AVAILABLE: BN = new BN(100000);
