const fs = require("fs")

const FILENAME = "C:/Work/Studying/TAIK/lab3/test.txt"

class Link {
  constructor(s, inp, stack, term = false) {
    this.s = s
    this.inp = inp
    this.stack = stack
    this.index = -1
    this.term = term
  }
}

class Command {
  constructor(f, values) {
    this.f = f
    this.values = values
  }
}

class Storage {
  constructor(filename) {
    this.P = new Set()
    this.H = new Set()
    this.s0 = "0"
    this.h0 = "|"
    this.empty_symbol = ""
    this.commands = []
    this.chain = []

    try {
      const fileData = fs.readFileSync(filename, "utf-8")
      const lines = fileData.split("\n")
      const pattern = /([A-Z])>([^\|]+(?:\|[^\|]+)*)/
      lines.forEach((line) => {
        line = line.trim()
        if (!line) return

        const match = line.match(pattern)
        if (!match) throw new Error("Не удалось распознать содержимое файла")

        const [_, symbol, transitions] = match
        this.H.add(symbol)
        const command = new Command(
          { s: this.s0, p: this.empty_symbol, h: symbol },
          []
        )
        const values = transitions.split("|")
        values.forEach((val) => {
          val.split("").forEach((c) => this.P.add(c))
          command.values.push({
            s: this.s0,
            c: val.split('').reverse().join(''),
          })
        })
        this.commands.push(command)
      })

      this.H.forEach((c) => {
        this.P.delete(c)})

      this.P.forEach((c) => {
        this.commands.push(
          new Command({ s: this.s0, p: c, h: c }, [
            { s: this.s0, c: this.empty_symbol },
          ])
        )
      })


      this.commands.push(
        new Command({ s: this.s0, p: this.empty_symbol, h: this.h0 }, [
          { s: this.s0, c: this.empty_symbol },
        ])
      )
    } catch (error) {
      throw new Error("Не удалось открыть файл для чтения")
    }
  }

  showInfo() {
    console.log("Входной алфавит:\nP = {", [...this.P].join(", "), "}\n")
    console.log(
      "Алфавит магазинных символов:\nZ = {",
      [...this.H, ...this.P].join(", "),
      ", h0}\n"
    )
    console.log("Список команд:")
    this.commands.forEach((cmd) => {
      const p = cmd.f.p === this.empty_symbol ? "lambda" : cmd.f.p
      const h = cmd.f.h === this.h0 ? "h0" : cmd.f.h
      const valuesStr = cmd.values
        .map((v) => `(s${v.s}, ${v.c === this.empty_symbol ? "lambda" : v.c})`)
        .join("; ")
      console.log(`f(s${cmd.f.s}, ${p}, ${h}) = {${valuesStr}}`)
    })
  }

  showChain() {
    console.log("\nЦепочка конфигураций: ")
    this.chain.forEach((link) => {
      const inpStr = link.inp || "lambda"
      console.log(`(s${link.s}, ${inpStr}, h0${link.stack}) |– `, "")
    })
    console.log("(s0, lambda, lambda)")
  }

  pushLink() {
    for (const cmd of this.commands) {
      const lastLink = this.chain[this.chain.length - 1]
      if (
        lastLink.inp &&
        lastLink.stack &&
        lastLink.s === cmd.f.s &&
        (lastLink.inp[0] === cmd.f.p || cmd.f.p === this.empty_symbol) &&
        lastLink.stack.slice(-1) === cmd.f.h
      ) {
        for (const val of cmd.values) {
          const newLink = new Link(val.s, lastLink.inp, lastLink.stack)
          if (cmd.f.p !== this.empty_symbol) {
            newLink.inp = newLink.inp.slice(1)
          }

          newLink.stack = newLink.stack.slice(0, -1) + val.c
          this.chain.push(newLink)

          if (newLink.inp.length < newLink.stack.length) {
            this.chain.pop()
            continue
          }

          if ((!newLink.inp && !newLink.stack) || this.pushLink()) {
            return true
          }
        }

        this.chain.pop()
        return false
      }
    }
    return false
  }

  checkLine(inputStr) {
    this.chain.push(
      new Link(this.s0, inputStr, "", this.commands[0].values.length === 1)
    )
    this.chain[0].stack += this.commands[0].f.h
    const result = this.pushLink()

    if (result) {
      console.log("Валидная строка")
      this.showChain()
    } else {
      console.log("Невалидная строка")
    }

    this.chain = []
    return result
  }
}

function main() {
  const storage = new Storage(FILENAME)
  storage.showInfo()
  const readline = require("readline").createInterface({
    input: process.stdin,
    output: process.stdout,
  })

  const askUserInput = () => {
    readline.question("Введите строку: ", (userInput) => {
      storage.checkLine(userInput)
      console.log()
      askUserInput()
    })
  }

  askUserInput()
}

main()
