namespace aRevolucaoDosPetWeb.Util;

public static class ServiceMessages
{
    public static class Api
    {
        public const string EmptyResponse = "A API não retornou uma mensagem.";
        public const string PublicUnavailable = "Não foi possível consultar a API pública.";
        public const string PrivateUnavailable = "Não foi possível consultar a API protegida.";

        public static string RequestRejected(int statusCode) =>
            $"A API recusou a chamada ({statusCode}).";
    }

    public static class User
    {
        public const string RegistrationSuccess = "Conta criada com sucesso!";
        public const string ConnectionUnavailable = "Não foi possível conectar à API.";
        public const string InvalidMediaName = "O nome do arquivo enviado à API não é válido.";
        public const string MediaUrlMissing = "A API não retornou a URL da foto enviada.";
        public const string MediaUploadSuccess = "Foto enviada com sucesso.";
        public const string MediaUploadFailed = "Não foi possível enviar a foto. Verifique o arquivo e tente novamente.";
        public const string UpdatedProfileMissing = "A API não retornou o perfil atualizado.";
        public const string PhotoUpdateSuccess = "Foto atualizada com sucesso.";
        public const string PhotoUpdateFailed = "Não foi possível atualizar a foto. Verifique o arquivo e tente novamente.";
        public const string UpdatedUserMissing = "A API não retornou os dados atualizados.";
        public const string ProfileUpdateSuccess = "Perfil atualizado com sucesso.";
        public const string ProfileUpdateFailed = "Não foi possível atualizar o perfil.";

        public static string RegistrationFailed(int statusCode) =>
            $"Não foi possível criar a conta ({statusCode}).";

        public static string RequestRejected(int statusCode) =>
            $"A API recusou a solicitação ({statusCode}).";
    }

    public static class Auth
    {
        public const string UnknownAccessLevel = "-";
        public const string DefaultRole = "Usuário";
    }

    public static class Pet
    {
        public const string AuthenticationRequired = "É necessário informar um token de autenticação.";
        public const string ConnectionUnavailable = "Não foi possível conectar à API de pets.";
        public const string InvalidResponse = "A API retornou uma resposta inválida para os dados do pet.";
        public const string PetResponseMissing = "A API não retornou os dados do pet.";
        public const string RequestTimedOut = "A solicitação à API de pets excedeu o tempo limite.";
        public const string SearchResponseMissing = "A API não retornou os resultados da busca de pets.";

        public static string RequestRejected(int statusCode) =>
            $"A API de pets recusou a solicitação ({statusCode}).";
    }
}