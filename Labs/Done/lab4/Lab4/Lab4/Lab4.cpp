//#include <iostream>
//#include <map>
//#include <set>
//#include <string>
//#include <vector>
//#include <iterator>
//#include <algorithm>
//
//// Тип для списка строк
//using List = std::set<std::string>;
//
//// Грамматика
//std::map<std::string, std::vector<std::vector<std::string>>> grammar;
//
//std::map<std::string, List> FIRST_cache;
//std::map<std::string, List> FOLLOW_cache;
//
//// Функция для вычисления FIRST для нетерминала
//List FIRST(std::string nonTerm) {
//    if (FIRST_cache.find(nonTerm) != FIRST_cache.end()) {
//        return FIRST_cache[nonTerm]; // Если значение уже вычислено, возвращаем из кэша
//    }
//
//    List firstSet;
//
//    // Проходим по всем правилам грамматики для данного нетерминала
//    for (const auto& production : grammar[nonTerm]) {
//        if (production.empty()) {
//            continue;
//        }
//
//        auto item = production[0];
//        // Если первый символ — терминал, добавляем его в FIRST
//        if (grammar.find(item) == grammar.end()) {
//            firstSet.insert(item);
//        }
//        else {  // Если это нетерминал, рекурсивно находим его FIRST
//            List temp = FIRST(item);
//            firstSet.insert(temp.begin(), temp.end());
//        }
//    }
//
//    FIRST_cache[nonTerm] = firstSet;  // Сохраняем результат в кэш
//    return firstSet;
//}
//
//// Функция для вычисления FOLLOW для нетерминала
//List FOLLOW(std::string nonTerm) {
//    if (FOLLOW_cache.find(nonTerm) != FOLLOW_cache.end()) {
//        return FOLLOW_cache[nonTerm]; // Если значение уже вычислено, возвращаем из кэша
//    }
//
//    List followSet;
//
//    if (nonTerm == "<program>") {
//        followSet.insert("$"); // Стартовый нетерминал имеет $ в FOLLOW
//    }
//
//    // Проходим по грамматике и находим все места, где встречается этот нетерминал
//    for (const auto& rule : grammar) {
//        for (const auto& production : rule.second) {
//            auto it = std::find(production.begin(), production.end(), nonTerm);
//            if (it != production.end()) {
//                auto nextSymbol = std::next(it);
//                if (nextSymbol != production.end()) {
//                    // Если следующий символ — терминал, добавляем его в FOLLOW
//                    if (grammar.find(*nextSymbol) == grammar.end()) {
//                        followSet.insert(*nextSymbol);
//                    }
//                    else {  // Если это нетерминал, добавляем его FIRST в FOLLOW
//                        List temp = FIRST(*nextSymbol);
//                        followSet.insert(temp.begin(), temp.end());
//                    }
//                }
//                else {
//                    if (rule.first != nonTerm) {
//                        List parentFollow = FOLLOW(rule.first);
//                        followSet.insert(parentFollow.begin(), parentFollow.end());
//                    }
//                }
//            }
//        }
//    }
//
//    FOLLOW_cache[nonTerm] = followSet;  // Сохраняем результат в кэш
//    return followSet;
//}
//
//// Функция для вывода множества
//void printSet(const List& lst) {
//    for (const auto& item : lst) {
//        std::cout << item << " ";
//    }
//    std::cout << std::endl;
//}
//
//void setGrammer()
//{
//    grammar["<program>"] = { {"<type>", "main", "(", ")", "{", "<statement>", "}"} };
//    grammar["<type>"] = { {"int"}, {"bool"}, {"void"} };
//    grammar["<statement>"] = {
//            {"<declaration>", ";"}, {"{", "<statement>", "}"},
//            {"<for>", "<statement>"}, {"<if>", "<statement>"}, {"<return>"}
//    };
//    grammar["<declaration>"] = { {"<type>", "<identifier>", "<assign>"} };
//    grammar["<identifier>"] = { {"<character>", "<id_end>"} };
//    grammar["<character>"] = { { "a" }, { "b" }, { "c" }, { "d" }, { "e" }, { "f" }, { "g" }, { "h" }, { "i" }, { "j" }, { "k" }, { "l" }, { "m" }, { "n" }, { "o" }, { "p" }, { "q" }, { "r" }, { "s" }, { "t" }, { "u" }, { "v" }, { "w" }, { "x" }, { "y" }, { "z" }, { "A" }, { "B" }, { "C" }, { "D" }, { "E" }, { "F" }, { "G" }, { "H" }, { "I" }, { "J" }, { "K" }, { "L" }, { "M" }, { "N" }, { "O" }, { "P" }, { "Q" }, { "R" }, { "S" }, { "T" }, { "U" }, { "V" }, { "W" }, { "X" }, { "Y" }, { "Z" }, { "_" } };
//    grammar["<id_end>"] = { {"<character>", "<id_end>"} };
//    grammar["<assign>"] = { {"=", "<assign_end>"} };
//    grammar["<assign_end>"] = { {"<identifier>"}, {"<number>" }};
//    grammar["<number>"] = {{"<digit>", "<number_end>"}};
//    grammar["<digit>"] = {{"0"}, {"1"}, {"2"}, {"3"}, {"4"}, {"5"}, {"6"}, {"7"}, {"8"}, {"9"}};
//    grammar["<number_end>"] = {{"<digit>", "<number_end>" }};
//    grammar["<for>"] = {{"for", "(", "<declaration>", ";", "<bool_expression>", ";", ")"}};
//    grammar["<bool_expression>"] = { {"<identifier>", "<relop>", "<identifier>"}, {"<number>", "<relop>", "<identifier>" }
//};
//    grammar["<relop>"] = { {"<"}, {">"}, {"=="}, {"!="} };
//    grammar["<if>"] = {{"if", "(", "<bool_expression>", ")"}};
//    grammar["<return>"] = { { "return", "<number>", ";" } };
//}
//
//int main() {
//    
//    setGrammer();
//
//    for (auto const& [key, val] : grammar)
//    {
//        std::cout << key << ": ";
//        printSet(FOLLOW(key));
//    }
//
//    /*std::string nonTerm = "<program>";
//    std::cout << "FIRST(<program>) = ";
//    printSet(FIRST(nonTerm));
//
//    std::cout << "FOLLOW(<program>) = ";
//    printSet(FOLLOW(nonTerm));
//
//    nonTerm = "<statement>";
//    std::cout << "FIRST(<statement>) = ";
//    printSet(FIRST(nonTerm));
//
//    std::cout << "FOLLOW(<statement>) = ";
//    printSet(FOLLOW(nonTerm));*/
//
//    return 0;
//}




#include <iostream>
#include <fstream>
#include <stack>
#include <string>
#include <cctype>
#include <map>
#include <set>
#include <vector>
#include <iterator>
#include <algorithm>


// Таблица предиктивного анализа
std::map<std::string, std::map<std::string, std::string>> parseTable;

// Синхронизирующие токены
std::set<std::string> syncTokens = { ")", ";", "}" };

// Тип для списка строк
using List = std::set<std::string>;

// Грамматика
std::map<std::string, std::vector<std::vector<std::string>>> grammar;

std::map<std::string, List> FIRST_cache;
std::map<std::string, List> FOLLOW_cache;

// Функция для вычисления FIRST для нетерминала
List FIRST(std::string nonTerm) {
    if (FIRST_cache.find(nonTerm) != FIRST_cache.end()) {
        return FIRST_cache[nonTerm]; // Если значение уже вычислено, возвращаем из кэша
    }

    List firstSet;

    // Проходим по всем правилам грамматики для данного нетерминала
    for (const auto& production : grammar[nonTerm]) {
        if (production.empty()) {
            continue;
        }

        auto item = production[0];
        // Если первый символ — терминал, добавляем его в FIRST
        if (grammar.find(item) == grammar.end()) {
            firstSet.insert(item);
        }
        else {  // Если это нетерминал, рекурсивно находим его FIRST
            List temp = FIRST(item);
            firstSet.insert(temp.begin(), temp.end());
        }
    }

    FIRST_cache[nonTerm] = firstSet;  // Сохраняем результат в кэш
    return firstSet;
}

// Функция для вычисления FOLLOW для нетерминала
List FOLLOW(std::string nonTerm) {
    if (FOLLOW_cache.find(nonTerm) != FOLLOW_cache.end()) {
        return FOLLOW_cache[nonTerm]; // Если значение уже вычислено, возвращаем из кэша
    }

    List followSet;

    if (nonTerm == "<program>") {
        followSet.insert("$"); // Стартовый нетерминал имеет $ в FOLLOW
    }

    // Проходим по грамматике и находим все места, где встречается этот нетерминал
    for (const auto& rule : grammar) {
        for (const auto& production : rule.second) {
            auto it = std::find(production.begin(), production.end(), nonTerm);
            if (it != production.end()) {
                auto nextSymbol = std::next(it);
                if (nextSymbol != production.end()) {
                    // Если nextSymbol терминал, добавляем его в set
                    if (grammar.find(*nextSymbol) == grammar.end()) {
                        followSet.insert(*nextSymbol);
                    }
                    else {  // Если nextSymbol нетерминал, берем его FIRST в FOLLOW
                        List temp = FIRST(*nextSymbol);
                        followSet.insert(temp.begin(), temp.end());
                    }
                }
                // Если следующего символа нет
                else {
                    if (rule.first != nonTerm) {
                        List parentFollow = FOLLOW(rule.first);
                        followSet.insert(parentFollow.begin(), parentFollow.end());
                    }
                }
            }
        }
    }

    FOLLOW_cache[nonTerm] = followSet;
    return followSet;
}

// Проверка терминала
bool isTerminal(const std::string& symbol) {
    return !symbol.empty() && (islower(symbol[0]) || ispunct(symbol[0]));
}

// Вывод ошибок
void reportError(const std::string& message, int line) {
    std::cout << "Error: " << message << " on line " << line << std::endl;
}

// Нерекурсивный предиктивный анализ
void predictiveParse(std::vector<std::string>& tokens) {
    std::stack<std::string> parseStack;
    parseStack.push("$");
    parseStack.push("<program>");

    size_t currentTokenIndex = 0;

    while (!parseStack.empty()) {
        std::string top = parseStack.top();
        parseStack.pop();

        if (isTerminal(top)) {
            if (top == tokens[currentTokenIndex]) {
                ++currentTokenIndex; // Совпадение
            }
            else {
                reportError("Incorrect symbol '" + tokens[currentTokenIndex] + "'", currentTokenIndex + 1);
                // Восстановление в режиме паники
                while (currentTokenIndex < tokens.size() && syncTokens.find(tokens[currentTokenIndex]) == syncTokens.end()) {
                    ++currentTokenIndex;
                }
            }
        }
        else if (parseTable[top].count(tokens[currentTokenIndex])) {
            std::string production = parseTable[top][tokens[currentTokenIndex]];
            if (production != "$") { // Не пустая продукция
                for (auto it = production.rbegin(); it != production.rend(); ++it) {
                    parseStack.push(std::string(1, *it));
                }
            }
        }
        else {
            reportError("Incorrect strcut " + top, currentTokenIndex + 1);
            // Восстановление в режиме паники
            while (currentTokenIndex < tokens.size() && syncTokens.find(tokens[currentTokenIndex]) == syncTokens.end()) {
                ++currentTokenIndex;
            }
        }
    }
}

void setGrammer()
{
    grammar["<program>"] = { {"<type>", "main", "(", ")", "{", "<statement>", "}"} };
    grammar["<type>"] = { {"int"}, {"bool"}, {"void"} };
    grammar["<statement>"] = {
            {"<declaration>", ";"}, {"{", "<statement>", "}"},
            {"<for>", "<statement>"}, {"<if>", "<statement>"}, {"<return>"}
    };
    grammar["<declaration>"] = { {"<type>", "<identifier>", "<assign>"} };
    grammar["<identifier>"] = { {"<character>", "<id_end>"} };
    grammar["<character>"] = { { "a" }, { "b" }, { "c" }, { "d" }, { "e" }, { "f" }, { "g" }, { "h" }, { "i" }, { "j" }, { "k" }, { "l" }, { "m" }, { "n" }, { "o" }, { "p" }, { "q" }, { "r" }, { "s" }, { "t" }, { "u" }, { "v" }, { "w" }, { "x" }, { "y" }, { "z" }, { "A" }, { "B" }, { "C" }, { "D" }, { "E" }, { "F" }, { "G" }, { "H" }, { "I" }, { "J" }, { "K" }, { "L" }, { "M" }, { "N" }, { "O" }, { "P" }, { "Q" }, { "R" }, { "S" }, { "T" }, { "U" }, { "V" }, { "W" }, { "X" }, { "Y" }, { "Z" }, { "_" } };
    grammar["<id_end>"] = { {"<character>", "<id_end>"} };
    grammar["<assign>"] = { {"=", "<assign_end>"} };
    grammar["<assign_end>"] = { {"<identifier>"}, {"<number>" }};
    grammar["<number>"] = {{"<digit>", "<number_end>"}};
    grammar["<digit>"] = {{"0"}, {"1"}, {"2"}, {"3"}, {"4"}, {"5"}, {"6"}, {"7"}, {"8"}, {"9"}};
    grammar["<number_end>"] = {{"<digit>", "<number_end>" }};
    grammar["<for>"] = {{"for", "(", "<declaration>", ";", "<bool_expression>", ";", ")"}};
    grammar["<bool_expression>"] = { {"<identifier>", "<relop>", "<identifier>"}, {"<number>", "<relop>", "<identifier>" }};
    grammar["<relop>"] = { {"<"}, {">"}, {"=="}, {"!="} };
    grammar["<if>"] = {{"if", "(", "<bool_expression>", ")"}};
    grammar["<return>"] = { { "return", "<number>", ";" } };
}

std::set<std::string> getTerm()
{
    std::set<std::string> terms;
    for (const auto& rule : grammar)
    {
        for (const auto& prod : rule.second)
        {
            for (const auto& item : prod)
            {
                if (item[0] != '<' && item[item.size() - 1] != '>')
                    terms.insert(item);
            }
        }
    }

    return terms;
}

std::string toString()

void setTable()
{
    for (const auto& [key, value] : grammar)
    {
        for (const auto& item : getTerm())
        {
            parseTable.insert({ key, {} });
            parseTable[key].insert({ item, "" });
        }
    }


    for (const auto& [nonterm, term_value] : parseTable)
    {
        auto prod = FIRST(nonterm); // terms
        for (const auto& [term, value] : parseTable[nonterm])
        {
            if (prod.contains(term))
                parseTable[nonterm][term] = grammar[]
        }
    }
}

int main() {
    setGrammer();
    setTable();

    // Чтение входного файла
    std::ifstream inputFile("input.txt");
    if (!inputFile.is_open()) {
        std::cerr << "Не удалось открыть файл." << std::endl;
        return 1;
    }

    std::vector<std::string> tokens;
    std::string token;
    while (inputFile >> token) {
        tokens.push_back(token);
    }
    tokens.push_back("$"); // Конец потока

    // Выполнение синтаксического анализа
    predictiveParse(tokens);

    return 0;
}
