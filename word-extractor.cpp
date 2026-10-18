#include <fstream>
#include <iostream>
#include <vector>
#include <string>
using namespace std;

vector<string> readFile(string FilePath)
{
    vector<string> extractedWords;
    string tempArray[] = { 0 };
    
    ifstream file(FilePath);
    
    if(!file.is_open())
    {
        vector<string> error = { "error opening file" };
        return  error;
    }
    
    for (int i = 0; file >> tempArray[i]; i++)
    {

    }
    
    return extractedWords;
}

int main()
{
    string filePath[] = { "/files/Articles1/Testing/2504NEWS.XML", "/files/Articles1/Testing/2538NEWS.XML"};
    vector<string> words;
    
    words = readFile(filePath[0]);
    
    std::cout << words;
    
    return 0;
}
