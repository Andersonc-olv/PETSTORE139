//Aula 24 - Conclusão API e Extração Token 02:12:41
//1 Bibliotecas
using RestSharp;
using Newtonsoft.Json;
using Models;

//2 Namespace
namespace Pet;

//3 Classe
public class PetTest
{
    //3.1 Atributos
    // Endereço da API
    private const string BASE_URL = "https://petstore.swagger.io/v2/";
    
    //3.2 Funções e Metodos

    //Função de leitura dos dados a partir de um arquivo csv
    public static IEnumerable<TestCaseData> getTestData()
    {
        String caminhoMassa = @"C:\Iterasys\PETSTORE139\Fixtures\Pets.csv";

        using var reader = new StreamReader(caminhoMassa);

        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(",");

            yield return new TestCaseData(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], values[5], values[6], values[7]);
        }
    }


    [Test, Order(1)]
    public void PostPetTest()
    {
        //Configura
        // instancia o objeto do tipo RestCLient com o endereço da API
        var client = new RestClient(BASE_URL);

        // instancia o objeto do tipo RestRequest com o complento de endereço como "pet" e configurando o metodo para ser um post (inclusão)
        var request = new RestRequest("pet", Method.Post);

        //armazena o conteudo do arquivo pet.json na memoria
        String jsonBody = File.ReadAllText(@"C:\Iterasys\PETSTORE139\Fixtures\Pet1.json");

        // adiciona na requesição o conteudo do arquivo Pet1.json
        request.AddBody(jsonBody);

        //Executa
        // executa a requisição conforme a configuração realizada
        // guarda o json retornado no objeto response
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        // exibe o response no console
        Console.WriteLine(responseBody);

        //Valide que na resposta, o status code é igual ao resultado esperado(200)
        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        //Valida o ID do animal
        int petId = responseBody.id;
        Assert.That(petId, Is.EqualTo(4914772));
        
        // VAlida o nome do animal na resposta
        String name = responseBody.name.ToString();
        Assert.That(name, Is.EqualTo("Bruce"));

        //Valida o status do animal
        String status = responseBody.status;
        Assert.That(status, Is.EqualTo("available"));

        //Armazenar os dados obtidos para usar nos proximos testes
        Environment.SetEnvironmentVariable("petId",petId.ToString());
    }

    [Test, Order(2)]
    public void GetPetTest()
    {
        // Configura
        int petId = 4914772;        //campo de pesquisa
        String petName = "Bruce";   //resultado esperado
        String categoryName = "dog";
        String tagsName = "vacinado";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Get);

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petId));
        Assert.That((string)responseBody.name, Is.EqualTo(petName));
        Assert.That((string)responseBody.category.name, Is.EqualTo(categoryName));
        Assert.That((string)responseBody.tags[0].name, Is.EqualTo(tagsName));
    }

    [Test, Order(3)]
    public void PutPetTest()
    {
        //Configura
        // Os dados de entrada vão formar o body da alteração
        //Vamos usar uma classe de modelo
        PetModel petModel = new PetModel();
        petModel.id = 4914772;
        petModel.category = new Category(1, "dog");
        petModel.name = "Bruce";
        petModel.photoUrls = new String[]{""};
        petModel.tags = new Tag[]{new Tag(1, "vacinado"), new Tag(2, "castrado")}; //petModel.tags = new Tag[]{new Tag(2, "castrado")};
        petModel.status = "pending";
        
        //Transforma o modelo acima em um arquvo json
        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);
        

        var client = new RestClient(BASE_URL);
        var request = new RestRequest("pet", Method.Put);
        request.AddBody(jsonBody);

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petModel.id));
        Assert.That((String)responseBody.tags[1].name, Is.EqualTo(petModel.tags[1].name));
        Assert.That((String)responseBody.status, Is.EqualTo(petModel.status));
    }

    [Test, Order(4)]
    public void DeletePetTest()
    {
        //Configura
        String petId = Environment.GetEnvironmentVariable("petId");

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Delete);

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        Assert.That((String)responseBody.message, Is.EqualTo(petId));
    }


    [TestCaseSource("getTestData", new object[]{}), Order(5)]
    public void PostPetDDTest(int petId, int categoryId, String categoryName, String petName, String photoUrls,String tagsIds, String tagsName, String status)
    {
        //Configura
        PetModel petModel = new PetModel();
        petModel.id = petId;
        petModel.category = new Category(categoryId, categoryName);
        petModel.name = petName;
        petModel.photoUrls = new String[]{photoUrls};

        // Codigo para gerar multiplas tags que o pet pode ter
        String[] tagsIdsList = tagsIds.Split(";");
        String[] tagsNameList = tagsName.Split(";");
        List<Tag> tagList = new List<Tag>();

        for (int i = 0; i < tagsIdsList.Length; i++)
        {
            int tagId = int.Parse(tagsIdsList[i]);
            String tagName = tagsNameList[i];

            Tag tag = new Tag(tagId, tagName);
            tagList.Add(tag);
        }
        
        petModel.tags = tagList.ToArray();

        petModel.status = status;

        // A estrutura de dados está pronta, agora vamos serealizar
        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);

        // instancia o objeto do tipo RestCLient com o endereço da API
        var client = new RestClient(BASE_URL);

        // instancia o objeto do tipo RestRequest com o complento de endereço como "pet" e configurando o metodo para ser um post (inclusão)
        var request = new RestRequest("pet", Method.Post);

        //armazena o conteudo do arquivo pet.json na memoria  String jsonBody = File.ReadAllText(@"C:\Iterasys\PETSTORE139\Fixtures\Pet1.json");

        // adiciona na requesição o conteudo do arquivo Pet1.json
        request.AddBody(jsonBody);

        //Executa
        // executa a requisição conforme a configuração realizada
        // guarda o json retornado no objeto response
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        // exibe o response no console
        Console.WriteLine(responseBody);

        //Valide que na resposta, o status code é igual ao resultado esperado(200)
        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        //Valida o ID do animal
        //int petId = responseBody.id;
        Assert.That((int)responseBody.id, Is.EqualTo(petId));
        
        // VAlida o nome do animal na resposta
        //String name = responseBody.name.ToString();
        Assert.That((String)responseBody.name, Is.EqualTo(petName));

        //Valida o status do animal
        //String status = responseBody.status;
        Assert.That((String)responseBody.status, Is.EqualTo(status));

        //Armazenar os dados obtidos para usar nos proximos testes
        //Environment.SetEnvironmentVariable("petId",petId.ToString());
    }

    [Test, Order(6)]
    public void GetUserLoginTest()
    {
        //Configura
        String username = "Natsu";
        String password = "teste";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"user/login?username={username}&password={password}", Method.Get);
        

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        String message = responseBody.message;
        String token = message.Substring(message.LastIndexOf(":")+1);
        Console.WriteLine($"Token = {token}");

        Environment.SetEnvironmentVariable("token", token);

    }
}