/**
 * Program IDL in camelCase format in order to be used in JS/TS.
 *
 * Note that this is only a type helper and is not the actual IDL. The original
 * IDL can be found at `target/idl/lumberjack.json`.
 */
export type Lumberjack = {
  address: "MkabCfyUD6rBTaYHpgKBBpBo5qzWA2pK2hrGGKMurJt";
  metadata: {
    name: "lumberjack";
    version: "0.1.0";
    spec: "0.1.0";
    description: "Created with Anchor";
  };
  instructions: [
    {
      name: "chopTree";
      discriminator: [120, 56, 196, 91, 213, 182, 36, 28];
      accounts: [
        {
          name: "sessionToken";
          optional: true;
        },
        {
          name: "player";
          writable: true;
          pda: {
            seeds: [
              {
                kind: "const";
                value: [112, 108, 97, 121, 101, 114];
              },
              {
                kind: "account";
                path: "player.authority";
                account: "playerData";
              }
            ];
          };
        },
        {
          name: "gameData";
          writable: true;
          pda: {
            seeds: [
              {
                kind: "arg";
                path: "levelSeed";
              }
            ];
          };
        },
        {
          name: "signer";
          writable: true;
          signer: true;
        },
        {
          name: "systemProgram";
          address: "11111111111111111111111111111111";
        }
      ];
      args: [
        {
          name: "levelSeed";
          type: "string";
        },
        {
          name: "counter";
          type: "u16";
        }
      ];
    },
    {
      name: "initPlayer";
      discriminator: [114, 27, 219, 144, 50, 15, 228, 66];
      accounts: [
        {
          name: "player";
          writable: true;
          pda: {
            seeds: [
              {
                kind: "const";
                value: [112, 108, 97, 121, 101, 114];
              },
              {
                kind: "account";
                path: "signer";
              }
            ];
          };
        },
        {
          name: "gameData";
          writable: true;
          pda: {
            seeds: [
              {
                kind: "arg";
                path: "levelSeed";
              }
            ];
          };
        },
        {
          name: "signer";
          writable: true;
          signer: true;
        },
        {
          name: "systemProgram";
          address: "11111111111111111111111111111111";
        }
      ];
      args: [
        {
          name: "levelSeed";
          type: "string";
        }
      ];
    }
  ];
  accounts: [
    {
      name: "gameData";
      discriminator: [237, 88, 58, 243, 16, 69, 238, 190];
    },
    {
      name: "playerData";
      discriminator: [197, 65, 216, 202, 43, 139, 147, 128];
    },
    {
      name: "sessionToken";
      discriminator: [233, 4, 115, 14, 46, 21, 1, 15];
    }
  ];
  errors: [
    {
      code: 6000;
      name: "notEnoughEnergy";
      msg: "Not enough energy";
    },
    {
      code: 6001;
      name: "wrongAuthority";
      msg: "Wrong Authority";
    }
  ];
  types: [
    {
      name: "gameData";
      type: {
        kind: "struct";
        fields: [
          {
            name: "totalWoodCollected";
            type: "u64";
          }
        ];
      };
    },
    {
      name: "playerData";
      type: {
        kind: "struct";
        fields: [
          {
            name: "authority";
            type: "pubkey";
          },
          {
            name: "name";
            type: "string";
          },
          {
            name: "level";
            type: "u8";
          },
          {
            name: "xp";
            type: "u64";
          },
          {
            name: "wood";
            type: "u64";
          },
          {
            name: "energy";
            type: "u64";
          },
          {
            name: "lastLogin";
            type: "i64";
          },
          {
            name: "lastId";
            type: "u16";
          }
        ];
      };
    },
    {
      name: "sessionToken";
      type: {
        kind: "struct";
        fields: [
          {
            name: "authority";
            type: "pubkey";
          },
          {
            name: "targetProgram";
            type: "pubkey";
          },
          {
            name: "sessionSigner";
            type: "pubkey";
          },
          {
            name: "validUntil";
            type: "i64";
          }
        ];
      };
    }
  ];
};
