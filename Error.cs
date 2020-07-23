using System;

namespace WhatsAppGroupAnalysis
{
    public class Error
    {
        public string Explanation { get; set; }

        public ErrorType ErrorType { get; set; }

        public Error (ErrorType errorType, Language errorLang = Language.En)
        {
            switch (errorLang)
            {
                case Language.En:
                    switch (errorType)
                    {
                        case ErrorType.FilePath:
                            Explanation = "The path to the indicated file is Invalid. Make sure the file path is the first argument passed to the program";
                            break;
                        case ErrorType.FilePermission:
                            Explanation = "The program does not have permission to read the file indicated. Make sure your user has access to that file.";
                            break;
                        case ErrorType.FileInvalid:
                            Explanation = "It was not possible to interpret the indicated file. Make sure you set the \"platform:\" parameter correctly.";
                            break;
                        case ErrorType.ParametersInvalid:
                            Explanation = "The parameters passed are invalid. Visit https://github.com/BernardoGomesNegri/WhatsappGroupAnalysis/blob/master/README.md for instructions on how to run this program";
                            break;
                        case ErrorType.Undefined:
                            Explanation = "There was an error. It was not possible to identify it.";
                            break;
                    }
                    break;
                case Language.Pt:
                    switch (errorType)
                    {
                        case ErrorType.FilePath:
                            Explanation = "O caminho até o arquivo é inválido! Tenha certeza que o caminho do arquivo é o primeiro parâmetro passado ao programa";
                            break;
                        case ErrorType.FilePermission:
                            Explanation = "O programa não tem permissão para ler o arquivo indicado. Tenha certeza que seu usuário tem acesso ao arquivo.";
                            break;
                        case ErrorType.FileInvalid:
                            Explanation = "Não foi possível interpretar o arquivo indicado. Verifique se você usou o parâmetro \"platform:\" corretamente.";
                            break;
                        case ErrorType.ParametersInvalid:
                            Explanation = "Os parâmetros passados não são válidos, visite: https://github.com/BernardoGomesNegri/WhatsappGroupAnalysis/blob/master/README.md para instruções em como rodar esse programa";
                            break;
                        case ErrorType.Undefined:
                            Explanation = "Houve um erro. Não foi possível identificá-lo";
                            break;
                        
                    }
                    break;
                default:
                    break;
            }
            errorType = this.ErrorType;
            Console.WriteLine(Explanation);
        }
    }
}
