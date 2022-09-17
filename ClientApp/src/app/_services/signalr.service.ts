import {Inject, Injectable} from "@angular/core"
import * as signalR from "@microsoft/signalr"
import {StorageService} from "./storage.service"

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private url: string
  public data: Map<string, Message[]> = new Map<string, Message[]>()
  public blocks: Block[] = []
  private canRequestMore: Map<string, boolean> = new Map<string, boolean>()
  private tooQuickRequests = false
  private firstRequest: Map<string, boolean> = new Map<string, boolean>()

  private hubConnection: signalR.HubConnection | undefined
  constructor(@Inject('BASE_URL') baseUrl: string, private storageService: StorageService) {
    this.url = baseUrl + 'chat'
    this.initiateConnection()
      .then(() => console.log('connection initiated'))
      .catch(err => console.log('error initiating connection: ' + err))
  }

  private async initiateConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.url + '?access_token=' + this.storageService.getToken().access_token)
      .build()
    await this.hubConnection.start()

    this.addGetChatsListener()
    this.addGetMessagesListener()
    this.addBroadcastMessageListener()
    this.addBroadcastEditListener()
    this.addBroadcastDeleteListener()

    this.getChats()
  }

  private getChats() {
    this.hubConnection?.invoke('GetChats').catch(err => console.log(err))
  }

  private addGetMessagesListener() {
    this.hubConnection?.on('GetMessages', (chatName: string, messages: Message[]) => {
      messages.reverse()
      this.data?.get(chatName)?.unshift(...messages)
      if (messages.length < 20) {
        this.canRequestMore.set(chatName, false)
      }
      const el = document.getElementsByClassName('messages')[0] as HTMLDivElement
      if (this.firstRequest.get(chatName)) {
        setTimeout(() => el.scrollTop = Math.max(0, el.scrollHeight - el.offsetHeight), 5)
        this.firstRequest.set(chatName, false)
      } else {
        let oldScrollTop = el.scrollTop
        let oldScrollHeight = el.scrollHeight
        setTimeout(() => el.scrollTop = oldScrollTop + (el.scrollHeight - oldScrollHeight), 5)
      }
    })
  }

  private addBroadcastMessageListener() {
    this.hubConnection?.on('BroadcastMessage', (chatName: string, message: Message) => {
      console.log('received ' + message.text + ' for chat ' + chatName)
      this.data.get(chatName)!.push(message)
      const el = document.getElementsByClassName('messages')[0] as HTMLDivElement
      if (Math.abs(el.scrollTop - (el.scrollHeight - el.offsetHeight)) < 15) {
        setTimeout(() => el.scrollTop = Math.max(0, el.scrollHeight - el.offsetHeight), 5)
      }
    })
  }

  private addGetChatsListener() {
    this.hubConnection?.on('GetChats', (data: Block[]) => {
      data.forEach((block: Block) => {
        this.data.set(block.chatName, [])
        this.canRequestMore.set(block.chatName, true)
        this.firstRequest.set(block.chatName, true)
      })
      this.blocks.push(...data)
    })
  }

  private addBroadcastEditListener() {
    this.hubConnection?.on('BroadcastEdit', (chatName: string, id: number, text: string) => {
      let message = this.data.get(chatName)!.find(m => m.id == id)
      if (message !== undefined) {
        message.text = text
      }
    })
  }

  public deleteMessageFromArray(chatName: string, id: number) {
    let array = this.data.get(chatName)!
    array.splice(array.findIndex(m => m.id == id), 1)
  }

  private addBroadcastDeleteListener() {
    this.hubConnection?.on('BroadcastDelete', (chatName: string, messageId: number) => {
      this.deleteMessageFromArray(chatName, messageId)
    })
  }

  public requestMessages(chat: string) {
    if (!this.canRequestMore.get(chat) || this.tooQuickRequests) {
      return
    }
    this.tooQuickRequests = true
    setTimeout(() => this.tooQuickRequests = false, 200)

    this.hubConnection?.invoke('GetMessages',
      this.data.get(chat)?.length, chat).catch(err => console.log(err))
  }

  public sendMessage(chat: string, messageText: string, replyTo: number) {
    console.log('sending ' + messageText + ' to chat ' + chat)
    this.hubConnection?.invoke('BroadcastMessage', chat, messageText, replyTo).catch(err => console.log(err))
  }

  public sendEditedMessage(id: number, messageText: string) {
    this.hubConnection?.invoke('BroadcastEdit', id, messageText).catch(err => console.log(err))
  }

  public deleteMessage(chatName: string, id: number, deleteForAll: boolean) {
    let len = this.data?.get(chatName)?.length
    if (len === undefined) {
      return
    }

    if (deleteForAll) {
      this.hubConnection?.invoke('BroadcastDelete', id).catch(err => console.log(err))
    } else {
      this.deleteMessageFromArray(chatName, id)
    }
  }
}
