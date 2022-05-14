using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using static BlazorApp1.Pages.Login;

namespace BlazorApp1.Pages
{
	public partial class DeviceList
	{
		/// <summary>
		/// 储存设备信息的列表
		/// </summary>
		List<string> _deviceList=new List<string>();
		/// <summary>
		/// 静态属性保存用户信息。
		/// </summary>
		public static UserInfo UserInfomation { get; set; } = new UserInfo();

		/*如果该页面不是使用程序跳转过来的，而是用户直接访问 URL 过来的
		 * 就重定向到 Login 页面*/
		[Inject]
		NavigationManager? Nav { get; set; }
		public static bool Initialized { get; set; } = false;
		protected override async Task OnInitializedAsync()
		{
			ESP32.Initialized = true;
			/*用户在该页面刷新后，会导致界面仍然停留在该页面，但是内存中的
			 所有数据都消失了，静态变量也恢复成初始值了。这个时候要重定向
			到登录页面重新登录*/
			if (!Initialized)
			{
				while (Nav == null)
				{
					await Task.Delay(100);
				}
				Nav.NavigateTo("/");
			}
			else
			{
				List<string>? list = await GetDeviceIdList();
				if(list != null)
				{
					_deviceList = list;
					StateHasChanged();
				}
			}
		}

		async Task<List<string>?> GetDeviceIdList()
		{
			HttpClient client = new();
			client.BaseAddress = new Uri(@"http://localhost:80");//末尾不需要带斜杠
			string json = JsonConvert.SerializeObject(UserInfomation);
			HttpContent content = new StringContent(json);
			HttpResponseMessage msg = await client.PostAsync("/getDeviceIdList", content);
			if (msg.StatusCode == System.Net.HttpStatusCode.OK)
			{
				string bodyString = await msg.Content.ReadAsStringAsync();
				Console.WriteLine(bodyString);
				return JsonConvert.DeserializeObject<List<string>>(bodyString);
			}
			return null;
		}
	}
}
