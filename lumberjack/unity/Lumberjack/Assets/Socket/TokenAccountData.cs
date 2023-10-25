using System;

namespace Socket
{
    [Serializable]
    public class TokenAccountData
    {
        public string program;
        public TokenParsed parsed;
        public long space;

        /*{
            "program": "spl-token",
            "parsed": {
                "info": {
                    "isNative": false,
                    "mint": "CgPG8inVvG3S6BpP6CWN1XY2swMXU1iPqgfYNaSMp5dd",
                    "owner": "u1SM97qygsy7VSFKzq62mLXHeF6L4VQ5jHssiuPhVEa",
                    "state": "initialized",
                    "tokenAmount": {
                        "amount": "6000000000",
                        "decimals": 9,
                        "uiAmount": 6.0,
                        "uiAmountString": "6"
                    }
                },
                "type": "account"
            },
            "space": 165
        }*/
    }

    [Serializable]
    public class TokenParsed
    {
        public TokenInfo info;
    }

    [Serializable]
    public class TokenInfo
    {
        public bool isNative;
        public string mint;
        public string owner;
        public string state;
        public TokenAmount tokenAmount; 
    }

    [Serializable]
    public class TokenAmount
    {
        public long amount;
        public int decimals;
        public float uiAmount;
        public string uiAmountString;
    }
}