using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OXF.Constants
{
    public static class Messages
    {
        public static class Login
        {
            public const string InvalidCredentials  = "Usuário ou senha inválidos.";
            public const string MaxAttemptsReached  = "Número máximo de tentativas atingido.";
            public const string UsernameRequired    = "Usuário é obrigatório.";
            public const string PasswordRequired    = "Senha é obrigatória.";
            public const string Success             = "Usuário cadastrado com sucesso!";
            public const string MaxAttempts         = "Número máximo de tentativas de cadastro atingido. Tente novamente mais tarde.";
            public const string FormInvalid         = "Por favor, corrija os erros no formulário.";
        }

        public static class api
        {
            public const string Success = "Usuário cadastrado com sucesso!";
            public const string UserExists = "Usuário já existe.";
            public const string APIConnectError = "Erro de conexão com a API:";
            public const string APIProcessDataError = "Erro ao processar os dados da API:";
        }

        public static class import
        {
            public const string Success         = "Produtos importados com sucesso!";
            public const string Error           = "Erro ao importar produto.";
            public const string InvalidData     = "Dados inválidos para importação.";
            public const string InvalidSession  = "Sessão expirada. Faça login novamente";
            public const string InvalidLine     = "Linha inválida. Esperado pelo menos 3 campos.";
            public const string InvalidLFormat  = "Formato inválido. Apenas arquivos .txt ou .csv são aceitos.";
            public const string FileRequired    = "Selecione um arquivo.";
            public const string ErrorImpBatch   = "Erro ao importar o lote";
            public const string SuccessFull     = "Importação concluída com sucesso. Total de produtos enviados:";
        }

        public static class product
        {
            public const string ErrorSelectProduct = "Erro ao consultar produtos:";
        }

        public static string ErrorProcess(string errorText)
        {
            return $"Erro: {errorText}";
        }

    }
}
