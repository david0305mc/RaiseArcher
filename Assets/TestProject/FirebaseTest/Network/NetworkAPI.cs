using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

using NetworkTest;
using System.Linq;
using System;
using UnityEngine.Purchasing;

public class NetworkAPI
{
    public static Dictionary<string, int> tableIdx = new Dictionary<string, int>()
    {
        {"DBVersion", 1},
        {"BaseData", 2},
        {"InventoryData", 3},
    };
    private static Dictionary<int, string> _tableNames;
    public static Dictionary<int, string> tableNames
    {
        get
        {
            if (_tableNames == null)
                _tableNames = tableIdx.ToDictionary(x => x.Value, x => x.Key);

            return _tableNames;
        }
    }
    public static string ConvertToTableName(int idx)
    {
        return tableNames.GetValueOrDefault(idx);
    }

    public static int ConvertToDateKey(string tableName)
    {
        return tableIdx.GetValueOrDefault(tableName);
    }

    public static async UniTask<EServerStatus> GetServerStatus(CancellationTokenSource _cts)
    {
        VersionData[] datas = await NetworkManager.Instance.GetRequest<VersionData[]>(string.Format("{0}/version_list.json", ServerSetting.commonUrl), _cts);
        Debug.Log(datas);

        if (datas != null && datas.Length > 0)
        {
            var orderedEnumerable = datas.OrderByDescending(d => d.version);

            //서버에 해당 버전 테이블이 없는경우 앱 강제 업데이트 요구됨
            //var versionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.version == BuildSetting.version);
            var versionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.version == 10500);
            ServerSetting.Set(versionData);

            //var lastVersionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.status == EServerStatus.Live);
            var lastVersionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode());
            ServerSetting.lastAppVersion = lastVersionData.version;
            Debug.LogFormat("[Server/Setting/LastAppVersion] {0}", ServerSetting.lastAppVersion);
        }

        return ServerSetting.status;
    }

    public class SignInReq
    {
        public int platform;
        /// <summary>플랫폼 아이디 (firebase)</summary>
        public string platform_id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string lang;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string push_id;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int os;
        public string gpresto_sdata;
        public int gpresto_engine_state;
    }

    public class SignInRes
    {
        public ulong uno;
        public string token;
        public string platform_id;
        /// <summary>탈퇴 유예시간 -> 0이 아닐 경우 탈퇴중 팝업 표시, 게임 로그인 진행하지 않음</summary>
        public double leave_time;
        public string country;
        /// <summary>최초 계정 생성여부 (0=재로그인, 1=최초 계정 생성)</summary>
        public int first_login;
    }

    public static async UniTask<SignInRes> SignIn(EPlatform _platform, string _firebaseToken, CancellationTokenSource _cts)
    {
        //string url = Utility.URLAntiCacheRandomizer(ServerSetting.gameUrl);
        SignInReq data = new SignInReq();
        data.platform = (int)_platform;
        data.platform_id = _firebaseToken;
        data.lang = "KO";
        data.push_id = string.Empty;
#if !UNITY_EDITOR && UNITY_ANDROID
        data.os = 1;
#elif !UNITY_EDITOR && UNITY_IOS
        data.os = 2;
#else
        data.os = 1;
#endif
        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.AUTH_USER_LOGIN, data);
        var res = await NetworkManager.Instance.SendToServer<SignInRes>(reqContext, _cts);
        Debug.Log(res);
        return res;
    }

    public class LoginReq
    {
        public ulong uno;
        public string token;
    }

    public class LoginRes
    {
        public string session;
    }

    public static async UniTask<LoginRes> Login(ulong uno, string token, CancellationTokenSource _cts)
    {
        var data = new LoginReq();
        data.uno = uno;
        data.token = token;

        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.AUTH_GAME_LOGIN, data);
        LoginRes res = await NetworkManager.Instance.SendToServer<LoginRes>(reqContext, _cts);
        
        ServerSetting.sess = res.session;
        Debug.Log("Session : " + res.session);
        Debug.Log("uNO : " + uno);
        return res;
    }

    public class BillingOrderReq
    {
        public int shop_platform;
        public int shop_idx;
        public string product_id;
        public string currency;
    }
    public class BillingOrderRes
    {
        public string bid;
        public int shop_idx;
        public string product_id;
    }

    public static async UniTask<BillingOrderRes> BillingMakeOrder(EStoreType shop_platform, int shop_idx, string product_id, string currency, CancellationTokenSource cts)
    {
        var data = new BillingOrderReq();
        data.shop_platform = (int)shop_platform;
        data.shop_idx = shop_idx;
        data.product_id = product_id;
        data.currency = currency;

        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.BILLING_INIT, data);
        return await NetworkManager.Instance.SendToServer<BillingOrderRes>(reqContext, cts);
    }

    public class BillingReceiptReq
    {
        public string bid;
        public int shop_platform;
        public int shop_idx;
        public string product_id;
        public string transaction_id;
        public string purchase_token;
        public string receipt;
    }
    public class BillingReceiptRes 
    {
        public string bid;
        public int shop_platform;
        public string product_id;
    }

    public static async UniTask<BillingReceiptRes> BillingVerifyReceipt(EStoreType storeType, string bid, string product_id, int shop_idx, Product product, CancellationTokenSource cts)
    {
        var data = new BillingReceiptReq();
        data.bid = bid;
        data.shop_platform = (int)storeType;
        data.shop_idx = shop_idx;
        data.product_id = product_id;

        if (!Application.isEditor)
        {
            var unityProduct = UnityProduct.Get(product);
            switch (storeType)
            {
                case EStoreType.Android:
                    data.transaction_id = unityProduct.transactionID;
                    data.purchase_token = unityProduct.receipt.PayLoadInfo.data.purchaseToken;
                    data.receipt = unityProduct.receipt.ToString();
                    break;
                case EStoreType.iOS:
                    data.transaction_id = unityProduct.transactionID;
                    data.purchase_token = "";
                    data.receipt = unityProduct.receipt.ToString();
                    break;
            }
        }

        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.BILLING_RECEIPT, data);
        return await NetworkManager.Instance.SendToServer<BillingReceiptRes>(reqContext, cts);
    }

    public class SaveDataRes
    {
        public class RawData
        {
            public string tableName;
            public int date_key;
            public string save_data;
        }

        public ulong ver;
        public List<RawData> save_datas;
    }

    public static async UniTask<SaveDataRes> LoadFromServer(CancellationTokenSource _cts)
    {
        var data = new Dictionary<string, List<int>>() { { "date_keys", new List<int>() } };
        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.USER_DATA_LOAD, data);
        SaveDataRes res = await NetworkManager.Instance.SendToServer<SaveDataRes>(reqContext, _cts);

        res.save_datas.ForEach(x => x.tableName = ConvertToTableName(x.date_key));
        //if (useCompress)
        if (true)
        {
            res.save_datas.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.save_data))
                    return;

                try
                {
                    x.save_data = CLZF2.DecompressFromBase64(x.save_data);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            });
        }
        return res;
    }

    public static async UniTask SaveToServer(CancellationTokenSource _cts = default)
    {
        UserDataManager.Instance.dbVersion.dbVersion = GameTime.Get();
        UserDataManager.Instance.SaveLocalData();
        SaveDataRes data = new SaveDataRes();
        data.ver = 999;
        data.save_datas = new List<SaveDataRes.RawData>();
        var tables = tableNames.Values.ToList();
        for (int i = 0; i < tableNames.Count; i++)
        {
            var date_key = ConvertToDateKey(tables[i]);
            var rawData = new SaveDataRes.RawData();
            rawData.tableName = tables[i];
            rawData.date_key = date_key;

            if (rawData.tableName == "DBVersion")
            {
                DBVersion dbVersion = new DBVersion();
                dbVersion.dbVersion = UserDataManager.Instance.dbVersion.dbVersion;
                rawData.save_data = dbVersion.ToJson();
            }
            else if (rawData.tableName == "BaseData")
            {
                rawData.save_data = UserDataManager.Instance.baseData.ToJson();
            }
            else if (rawData.tableName == "InventoryData")
            {
                rawData.save_data = UserDataManager.Instance.inventoryData.ToJson();
            }

            if (!string.IsNullOrEmpty(rawData.save_data))
            {
                rawData.save_data = CLZF2.CompressToBase64(rawData.save_data);
            }
            
            data.save_datas.Add(rawData);
        }
        
        var reqContext = NetworkTest.RequestContext.Create(ServerSetting.gameUrl, ServerCmd.USER_DATA_SAVE, data);
        await NetworkManager.Instance.SendToServer(reqContext, _cts);
    }
}
