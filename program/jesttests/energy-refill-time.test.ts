import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { Lumberjack } from "../target/types/lumberjack";
import { Clock, startAnchor } from "solana-bankrun";
import { BankrunProvider } from "anchor-bankrun";

describe("Energy refill", () => {
  test("Turning forward time refills energy", async () => {
    const context = await startAnchor("", [], []);
    const client = context.banksClient;

    const provider = new BankrunProvider(context);
    anchor.setProvider(provider);

    const program = anchor.workspace.Lumberjack as Program<Lumberjack>;
    const payer = provider.wallet as anchor.Wallet;
    const gameDataSeed = "gameData";

    const [playerPDA] = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("player"), payer.publicKey.toBuffer()],
      program.programId
    );

    const [gameDataPDA] = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from(gameDataSeed)],
      program.programId
    );

    try {
      let tx = await program.methods
        .initPlayer(gameDataSeed)
        .accountsStrict({
          player: playerPDA,
          signer: payer.publicKey,
          gameData: gameDataPDA,
          systemProgram: anchor.web3.SystemProgram.programId,
        })
        .rpc({ skipPreflight: true });
      console.log("Init transaction", tx);

      console.log("Confirmed", tx);
    } catch (e) {
      console.log("Player already exists: ", e);
    }

    // Spend 11 energy
    for (let i = 0; i < 11; i++) {
      console.log(`Chop instruction ${i}`);

      let tx = await program.methods
        .chopTree(gameDataSeed, i)
        .accountsStrict({
          player: playerPDA,
          sessionToken: null,
          signer: payer.publicKey,
          gameData: gameDataPDA,
          systemProgram: anchor.web3.SystemProgram.programId,
        })
        .rpc();

      await console.log("Chop instruction", tx);
    }

    await client.getAccount(playerPDA).then((info) => {
      const decoded = program.coder.accounts.decode(
        "playerData",
        Buffer.from(info.data)
      );
      console.log("Player account info", JSON.stringify(decoded));
      expect(decoded).toBeDefined();
      expect(parseInt(decoded.energy)).toEqual(89);
    });

    const timestamp = Math.floor(Date.now() / 1000);

    // Turn forward the clock for 11 minutes
    const currentClock = await client.getClock();
    context.setClock(
      new Clock(
        currentClock.slot,
        currentClock.epochStartTimestamp,
        currentClock.epoch,
        currentClock.leaderScheduleEpoch,
        BigInt(timestamp) + BigInt(60 * 11)
      )
    );

    // Chop another tree, so that the energy is updated in the account.
    // (Usually the client predicts the time and updates the energy)
    let tx = await program.methods
      .chopTree(gameDataSeed, 0)
      .accountsStrict({
        player: playerPDA,
        sessionToken: null,
        signer: payer.publicKey,
        gameData: gameDataPDA,
        systemProgram: anchor.web3.SystemProgram.programId,
      })
      .rpc();

    // Get the account again and check if the energy is updated.
    // Its 99 because the last chop also costs on energy again.
    await client.getAccount(playerPDA).then((info) => {
      const decoded = program.coder.accounts.decode(
        "playerData",
        Buffer.from(info.data)
      );
      console.log("Player account info", JSON.stringify(decoded));
      expect(decoded).toBeDefined();
      expect(parseInt(decoded.energy)).toEqual(99);
    });
  });
});
