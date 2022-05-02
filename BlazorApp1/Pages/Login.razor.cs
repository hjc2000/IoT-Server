using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace BlazorApp1.Pages
{
	public partial class Login
	{
		[Inject]
		[NotNull]
		private MessageService? MessageService { get; set; }
		[NotNull]
		private Message? Message { get; set; }
		async void ShowMsg()
		{
			Message.SetPlacement(Placement.Top);
			await MessageService.Show(new MessageOption()
			{
				Content = "这是一条提示消息"
			});
		}
	}
}
