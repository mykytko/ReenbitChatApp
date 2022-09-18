interface Message {
  id: number
  username: string
  text: string
  date: string
  replyTo: number
  replyIsPersonal: boolean
}

interface Block {
  chatName: string
  lastMessageSender: string
  lastMessageText: string
  isPersonal: boolean
}
