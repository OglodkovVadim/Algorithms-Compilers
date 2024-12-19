const { createInterface } = require("readline")

const readlineInterface = createInterface({
  input: process.stdin,
  output: process.stdout,
})

const ERROR_MESSAGES = {
  INVALID_EXPRESSION: "Некорректное выражение!",
  LETTERS_NOT_ALLOWED: "Нельзя вводить буквы!",
  DIVISION_BY_ZERO: "Деление на ноль невозможно!",
  INVALID_PARENTHESIS: "Несоответствие скобок!",
  INVALID_NUMBER: "Некорректное число!",
}

class ExpressionCalculator {
  static isMathOperator(character) {
    return "+-*/()#^".includes(character)
  }

  static isWhitespace(character) {
    return character === " " || character === "\t"
  }

  static isNumeric(character) {
    return "0123456789.".includes(character)
  }

  static getOperatorPriority(operator) {
    switch (operator) {
      case "(":
        return 1
      case ")":
        return 1
      case "+":
        return 2
      case "-":
        return 2
      case "*":
        return 3
      case "/":
        return 3
      case "^":
        return 4
      default:
        return 0
    }
  }

  static isUnaryMinus(index, expression) {
    const isOperator = this.isMathOperator(expression[index - 1])
    const isNotRightParenthesis = expression[index - 1] !== ")"
    return (
      expression[index] === "-" &&
      (index === 0 || (isOperator && isNotRightParenthesis))
    )
  }

  static replaceNegativeNumbers(expression) {
    return expression.replace(/(?<!\w)-(\d+(\.\d+)?)/g, "(0 - $1)")
  }

  static replaceDoubleMinus(expression) {
    return expression.replace(/--/g, "+")
  }

  static removeExtraSpaces(expression) {
    return expression.replace(/\s+/g, "")
  }

  static replacePowFunction(expression) {
    return expression.replace(/pow\(([^()]+(?:\([^()]*\))?),([^()]+(?:\([^()]*\))?)\)/g, "($1^$2)")
  }

  static convertToPostfix(expression) {
    expression = this.removeExtraSpaces(expression)
    expression = this.replaceNegativeNumbers(expression)
    expression = this.replaceDoubleMinus(expression)
    expression = this.replacePowFunction(expression)

    if (expression.includes(".") && expression.includes("..")) {
      return ERROR_MESSAGES.INVALID_EXPRESSION
    }

    const leftParenthesisCount = (expression.match(/\(/g) || []).length
    const rightParenthesisCount = (expression.match(/\)/g) || []).length

    if (leftParenthesisCount !== rightParenthesisCount) {
      return ERROR_MESSAGES.INVALID_PARENTHESIS
    }

    let outputQueue = ""
    let operatorStack = []

    for (let i = 0; i < expression.length; i++) {
      if (!this.isNumeric(expression[i]) && !this.isMathOperator(expression[i]) && !this.isWhitespace(expression[i])) {
        return ERROR_MESSAGES.LETTERS_NOT_ALLOWED
      }

      if (this.isUnaryMinus(i, expression)) {
        outputQueue += expression[i++]

        while (i < expression.length && this.isNumeric(expression[i])) {
          outputQueue += expression[i++]
        }
        
        outputQueue += " "
        i--
      } else if (this.isNumeric(expression[i])) {
        while (i < expression.length && (this.isNumeric(expression[i]) || expression[i] === ".")) {
          outputQueue += expression[i++]
        }

        outputQueue += " "
        i--
      } else if (expression[i] === "(") { 
        operatorStack.push(expression[i])
      } else if (expression[i] === ")") {
        while (operatorStack.length && operatorStack[operatorStack.length - 1] !== "(") {
          outputQueue += `${operatorStack.pop()} `
        }

        operatorStack.pop()
      } else if (this.isMathOperator(expression[i])) {
        while (operatorStack.length && this.getOperatorPriority(expression[i]) <= this.getOperatorPriority(operatorStack[operatorStack.length - 1])) {
          outputQueue += `${operatorStack.pop()} `
        }

        operatorStack.push(expression[i])
      }
    }

    while (operatorStack.length) {
      outputQueue += operatorStack.pop() + " "
    }

    return outputQueue
  }

  static evaluatePostfix(postfixExpression) {
    let evaluationStack = []

    for (let i = 0; i < postfixExpression.length; i++) {
      if (this.isWhitespace(postfixExpression[i])) continue

      if (!this.isMathOperator(postfixExpression[i])) {
        let number = ""
        while (i < postfixExpression.length && !this.isMathOperator(postfixExpression[i]) && !this.isWhitespace(postfixExpression[i])) {
          if (!this.isNumeric(postfixExpression[i]) && postfixExpression[i] !== "-") {
            return ERROR_MESSAGES.LETTERS_NOT_ALLOWED
          }

          number += postfixExpression[i++]
        }

        evaluationStack.push(parseFloat(number))
        i--
      } else {
        const operandB = evaluationStack.pop()
        const operandA = evaluationStack.pop()
        let result

        switch (postfixExpression[i]) {
          case "+":
            result = operandA + operandB
            break

          case "-":
            result = operandA - operandB
            break

          case "*":
            result = operandA * operandB
            break

          case "/":
            if (operandB === 0) {
              console.log(ERROR_MESSAGES.DIVISION_BY_ZERO)
              return NaN
            }
            result = operandA / operandB
            break

          case "^":
            result = Math.pow(operandA, operandB)
            break
        }

        evaluationStack.push(result)
      }
    }

    return evaluationStack.length ? evaluationStack.pop() : NaN
  }

  static calculateExpression(inputExpression) {
    let postfixExpression = this.convertToPostfix(inputExpression)
    console.log("Преобразованное выражение (в постфиксной форме): " + postfixExpression)
    if (Object.values(ERROR_MESSAGES).includes(postfixExpression)) return NaN
    let result = this.evaluatePostfix(postfixExpression)
    return result
  }
}

function promptForExpression() {
  readlineInterface.question("Введите выражение: ", (input) => {
    if (!input) {
      readlineInterface.close()
    } else {
      console.log("Результат: " + ExpressionCalculator.calculateExpression(input))
      promptForExpression()
    }
  })
}

promptForExpression()
