using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Gley.EasyIAP;

namespace Services.InApp
{
    public static class InAppPurchasingService
    {
        public static async UniTask<bool> TryBuyConsumableAsync(ShopProductNames shopProduct)
        {
            var task = new UniTaskCompletionSource<bool>();

            
            
            API.BuyProduct(shopProduct,
                (status, message, product) => { task.TrySetResult(status == IAPOperationStatus.Success); });

            return await task.Task;
        }
    }
}