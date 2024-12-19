const transitionTable = new Map() // For storing transitions
const deterministicTable = new Map()
let unprocessedStates = []
let processedStates = new Set()
let inputSymbols = new Set()

function validateString(inputString) {
  let currentState = "q0"
  for (let idx = 0; idx < inputString.length; idx++) {
    const char = inputString[idx]
    const pair = `${currentState},${char}`

    if (!deterministicTable.has(pair)) {
      console.log(`Ошибка: переход ${pair} не найден.`)
      return false
    }

    currentState = deterministicTable.get(pair).values().next().value
    console.log(`Текущее состояние: ${currentState}`)

    if (currentState.includes("f") && idx === inputString.length - 1) {
      console.log(`Строка '${inputString}' корректна.`)
      return true
    }
  }
  return false
}

function mainLoop() {
  const readline = require("readline").createInterface({
    input: process.stdin,
    output: process.stdout,
  })

  console.log("Введите строку для проверки или exit для выхода:")

  readline.on("line", (userInput) => {
    if (userInput === "exit") {
      console.log("Работа завершена!")
      readline.close()
      return
    }

    if (validateString(userInput)) {
      console.log("Строка корректна.")
    } else {
      console.log("Строка некорректна.")
    }
  })
}

const fs = require("fs")

function readAutomaton(fileName) {
  try {
    const data = fs.readFileSync(fileName, "utf8")
    console.log("Чтение автомата из файла")

    const lines = data.split("\n")
    const pattern = /\s*q\s*(\d+)\s*,\s*(.+)\s*=\s*([qf])\s*(\d+)\s*/

    lines.forEach((line) => {
      const match = line.match(pattern)
      if (match) {
        const state = `q${match[1]}`
        let symbol = match[2].trim()
        const nextState = match[3] === "f" ? `f${match[4]}` : `q${match[4]}`

        if (!transitionTable.has(`${state},${symbol}`)) {
          transitionTable.set(`${state},${symbol}`, new Set())
        }
        transitionTable.get(`${state},${symbol}`).add(nextState)
        unprocessedStates.push(state)
        inputSymbols.add(symbol)
      } else {
        console.log("Ошибка при обработке строки")
      }
    })

    if (isDeterministic()) {
      console.log("Автомат детерминирован.")
      for (let [pair, resultState] of transitionTable.entries()) {
        deterministicTable.set(pair, resultState)
      }
    } else {
      console.log("Автомат не детерминирован.")
      determinize()
    }

    displayTransitionTable(deterministicTable)
  } catch (err) {
    console.log(`Файл '${fileName}' не найден.`)
    return false
  }
  return true
}

function isDeterministic() {
  for (let transitions of transitionTable.values()) {
    if (transitions.size > 1) {
      return false
    }
  }
  return true
}

function determinize() {
  while (unprocessedStates.length) {
    const currentState = unprocessedStates.shift()
    inputSymbols.forEach((symbol) => {
      const pair = `${currentState},${symbol}`
      if (!transitionTable.has(pair)) return

      const resultingStates = transitionTable.get(pair)
      const newState = createNewState(resultingStates)

      if (!processedStates.has(newState)) {
        unprocessedStates.push(newState)
      }

      deterministicTable.set(pair, new Set().add(newState))
    })

    processedStates.add(currentState)
  }
}

function createNewState(stateSet) {
  const combinedName = Array.from(stateSet).sort().join("")

  if (!transitionTable.has(`${combinedName}`)) {
    transitionTable.set(`${combinedName}`, new Set())
  }

  inputSymbols.forEach((symbol) => {
    const newTransitionSet = new Set()

    stateSet.forEach((state) => {
      const pair = `${state},${symbol}`
      if (transitionTable.has(pair)) {
        transitionTable
          .get(pair)
          .forEach((result) => newTransitionSet.add(result))
      }
    })

    if (newTransitionSet.size > 0) {
      transitionTable.set(`${combinedName},${symbol}`, newTransitionSet)
    }
  })

  return combinedName
}

function displayTransitionTable(table) {
  console.log("Таблица переходов:")
  for (let [pair, resultState] of table) {
    console.log(pair, "->", ...resultState)
  }
  console.log()
}

const fileName = "C:/Work/Studying/TAIK/lab2/test.txt"

if (readAutomaton(fileName)) {
  mainLoop()
}
