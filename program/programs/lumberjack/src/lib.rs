pub use crate::errors::GameErrorCode;
pub use anchor_lang::prelude::*;
pub use session_keys::{ session_auth_or, Session, SessionError };
pub mod constants;
pub mod errors;
pub mod instructions;
pub mod state;
use instructions::*;

#[cfg(not(feature = "no-entrypoint"))]
use solana_security_txt::security_txt;

declare_id!("MkabCfyUD6rBTaYHpgKBBpBo5qzWA2pK2hrGGKMurJt");

#[cfg(not(feature = "no-entrypoint"))]
security_txt! {
    // Required fields
    name: "Solana Game Preset",
    project_url: "https://github.com/solana-developers/solana-game-preset",
    contacts: "email:Dev Rel <devrel@solana.org>, twitter:https://x.com/solana_devs",
    policy: "There bug bounties in this repository, but PRs are welcome. :)",

    // Optional Fields
    preferred_languages: "en,de",
    source_code: "https://github.com/solana-developers/solana-game-preset",
    source_revision: "5vJwnLeyjV8uNJSp1zn7VLW8GwiQbcsQbGaVSwRmkE4r",
    source_release: "",
    encryption: "",
    auditors: "Verifier pubkey: 5vJwnLeyjV8uNJSp1zn7VLW8GwiQbcsQbGaVSwRmkE4r",
    acknowledgements: "Thanks to all the contributors!"
}

#[program]
pub mod lumberjack {
    use super::*;

    pub fn init_player(ctx: Context<InitPlayer>, _level_seed: String) -> Result<()> {
        init_player::init_player(ctx)
    }

    // This function lets the player chop a tree and get 1 wood. The session_auth_or macro
    // lets the player either use their session token or their main wallet. (The counter is only
    // there so that the player can do multiple transactions in the same block. Without it multiple transactions
    // in the same block would result in the same signature and therefore fail.)
    #[session_auth_or(
        ctx.accounts.player.authority.key() == ctx.accounts.signer.key(),
        GameErrorCode::WrongAuthority
    )]
    pub fn chop_tree(ctx: Context<ChopTree>, _level_seed: String, counter: u16) -> Result<()> {
        chop_tree::chop_tree(ctx, counter, 1)
    }
}
