pub use crate::errors::GameErrorCode;
pub use anchor_lang::prelude::*;
pub use session_keys::{session_auth_or, Session, SessionError};
pub mod constants;
pub mod errors;
pub mod instructions;
pub use instructions::*;
pub mod state;

declare_id!("MkabCfyUD6rBTaYHpgKBBpBo5qzWA2pK2hrGGKMurJt");

#[program]
pub mod lumberjack {

    use super::*;

    pub fn init_player(ctx: Context<InitPlayer>) -> Result<()> {
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
    pub fn chop_tree(ctx: Context<ChopTree>, counter: u16, seed: String) -> Result<()> {
        chop_tree::chop_tree(ctx, counter)
    }
}
