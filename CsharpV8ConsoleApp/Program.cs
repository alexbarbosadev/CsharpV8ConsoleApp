using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsharpV8ConsoleApp
{
	class Program
	{
		private static int _velocimetro = 0;
		private static object _cadeado = new object();
		private static DateTime _cronometroInicio;
		private static DateTime _cronometroFim;

		static async Task Main(string[] args)
		{
			var tarefas = new List<Task>();

			_cronometroInicio = DateTime.Now;
			for (var i = 0; i < 8; i++)
			{
				var tarefa = Task.Run(async () =>
				{
					for (var contador = 0; contador < 200; contador++)
					{
						var paginaHtml = await ObterPaginaHtmlAsync("https://www.youtube.com");
						var imagensEncontradas = ExtrairImagens(paginaHtml);
						await SalvarImagens(imagensEncontradas);
						if (_velocimetro >= 100)
							break;
					}
				});
				tarefas.Add(tarefa);
			}

			await Task.WhenAll(tarefas);
			var resultadoCronometro = _cronometroFim - _cronometroInicio;

			Console.WriteLine("================================================");
			Console.WriteLine($"0 a 100 V8: {resultadoCronometro.TotalSeconds}s");
			Console.WriteLine("================================================");
		}

		private static async Task<string> ObterPaginaHtmlAsync(string uri)
		{
			using (var httpClient = new HttpClient())
			{
				var paginaHtml = await httpClient.GetStringAsync(uri);
				return paginaHtml;
			}
		}

		private static IList<string> ExtrairImagens(string paginaHtml)
		{
			var regexConsultaImagem = new Regex(@"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?\.jpg");
			var imagensEncontradas = regexConsultaImagem.Matches(paginaHtml);
			return imagensEncontradas.Select(x => x.Value).ToList();
		}

		static async Task SalvarImagens(IList<string> imagensUtl)
		{
			var nomeDiretorio = "img";
			if (!Directory.Exists(nomeDiretorio))
				Directory.CreateDirectory(nomeDiretorio);

			using (var httpClien = new HttpClient())
			{
				foreach (var imgUrl in imagensUtl)
				{
					if (_velocimetro == 100)
						_cronometroFim = DateTime.Now;

					if (_velocimetro >= 100)
						break;

					Console.WriteLine($"Baixando imagem: {imgUrl}");
					var nomeImagem = Path.Combine(nomeDiretorio, $"{Guid.NewGuid()}.jpg");
					var img = await httpClien.GetByteArrayAsync(imgUrl);
					await File.WriteAllBytesAsync(nomeImagem, img);

					lock (_cadeado)
					{
						_velocimetro++;
					}
				}
			}
		}
	}
}
