using NSE.WebApp.MVC.Models;
using Microsoft.Extensions.Options;
using NSE.WebApp.MVC.Extensions;
using static NSE.WebAPI.Core.Controllers.MainController;

namespace NSE.WebApp.MVC.Services
{
    public class AutenticacaoService : Service, IAutenticacaoService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<AppSettings> _settings;

        public AutenticacaoService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings;
            _httpClient.BaseAddress = new Uri(_settings.Value.AutenticacaoUrl);
        }

        public async Task<UsuarioRespostaLogin> Login(UsuarioLogin usuarioLogin)
        {
            var loginContent = ObterConteudo(usuarioLogin);

            var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}api/identidade/autenticar", loginContent);

            if (!TratarErrosResponse(response))
            {
                return new UsuarioRespostaLogin()
                {
                    ResponseResult = await DeserializeObjectResponse<WebAPI.Core.Controllers.MainController.ResponseResult>(response)
                };
            }

            return await DeserializeObjectResponse<UsuarioRespostaLogin>(response);
        }

        public async Task<UsuarioRespostaLogin> Registro(UsuarioRegistro usuarioRegistro)
        {
            var registroContent = ObterConteudo(usuarioRegistro);

            var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}api/identidade/nova-conta", registroContent);

            if (!TratarErrosResponse(response))
            {
                return new UsuarioRespostaLogin()
                {
                    ResponseResult = await DeserializeObjectResponse<WebAPI.Core.Controllers.MainController.ResponseResult>(response)
                };
            }

            return await DeserializeObjectResponse<UsuarioRespostaLogin>(response);
        }
    }
}
