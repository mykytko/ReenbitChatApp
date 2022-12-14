import {Component, Inject, OnInit} from '@angular/core'
import {SignalrService} from "../_services/signalr.service"
import {HttpClient} from "@angular/common/http"
import {StorageService} from "../_services/storage.service"

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  selectedChat: string = ""
  url: string
  messageText: string = ""
  isPersonal: Boolean = false
  scrolls: Map<string, number> = new Map<string, number>()
  private nextEnterListenerIsSend = false
  private replyTo = -1
  private replyIsPersonal = false

  private scrollListener = function(this: HomeComponent, event: Event) {
    event.preventDefault()
    let el = document.getElementsByClassName('messages')[0]
    if (el.scrollTop < 15 ) {
      this.signalrService.requestMessages(this.selectedChat)
    }
  }

  private sendListener = function(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      event.preventDefault()
      document.getElementById('send')!.click()
    }
  }

  private editListener = function(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      event.preventDefault()
      document.getElementById('edit')!.click()
    }
  }

  constructor(public signalrService: SignalrService,
              private http: HttpClient,
              @Inject('BASE_URL') baseUrl: string,
              public storageService: StorageService) {
    this.url = baseUrl
  }

  public ngOnInit() {
    document.getElementById('message-text')!.addEventListener('keypress', this.sendListener)
    document.getElementsByClassName('messages')[0].addEventListener('scroll',
      this.scrollListener.bind(this))
  }

  private toggleEnterListener() {
    let textField = document.getElementById('message-text')!
    if (this.nextEnterListenerIsSend) {
      textField.removeEventListener('keypress', this.editListener)
      textField.addEventListener('keypress', this.sendListener)
    } else {
      textField.removeEventListener('keypress', this.sendListener)
      textField.addEventListener('keypress', this.editListener)
    }

    this.nextEnterListenerIsSend = !this.nextEnterListenerIsSend
  }

  public openChat(chatName: string) {
    let messages = document.getElementsByClassName('messages')[0]
    let scroll = this.scrolls.get(chatName)
    if (this.selectedChat !== undefined) {
      this.scrolls.set(this.selectedChat, messages.scrollTop)
      if (scroll !== undefined) {
        setTimeout(() => messages.scrollTop = scroll!, 5)
      }
    }

    this.selectedChat = chatName
    if (scroll === undefined || scroll < 15) {
      this.signalrService.requestMessages(chatName)
    }
    this.isPersonal = this.signalrService.blocks.find(b => b.chatName == chatName)!.isPersonal
  }

  public sendMessage() {
    if (this.selectedChat === undefined || this.selectedChat === '' || this.messageText === '') {
      return
    }

    this.signalrService.sendMessage(this.selectedChat, this.messageText, this.replyTo, this.replyIsPersonal)
    this.messageText = ''
    this.cancelReply()
  }

  public edit(message: Message) {
    this.toggleEnterListener()
    let oldText = this.messageText
    this.messageText = message.text
    let sendButton = document.getElementById('send')!
    let editButton = document.getElementById('edit')!

    sendButton.style.display = 'none'
    editButton.style.display = 'block'

    editButton.onclick = () => {
      this.signalrService.sendEditedMessage(message.id, this.messageText)
      this.messageText = oldText
      editButton.style.display = 'none'
      sendButton.style.display = 'block'
      this.toggleEnterListener()
    }
  }

  public delete(message: Message) {
    if (message.username !== this.storageService.getToken().username) {
      if (confirm('Do you want to delete this message from your client?')) {
        this.signalrService.deleteMessageFromArray(this.selectedChat, message.id)
      }

      return
    }

    let dialog = document.getElementById("dialog") as HTMLDialogElement
    dialog.showModal()
    document.getElementById('dialog-cancel')!.onclick = () => dialog.close()
    document.getElementById('dialog-delete')!.onclick = () => {
      let deleteForAll = (document.getElementById('delete-for-all')! as HTMLInputElement).checked
      this.signalrService.deleteMessage(this.selectedChat, message.id, deleteForAll)
      dialog.close()
    }
  }

  public reply(id: number) {
    let replyDialog = document.getElementById('reply-dialog')! as HTMLDialogElement
    let callback = () => {
      replyDialog.close()
      let reply = document.getElementById('reply')!
      let message = this.signalrService.data.get(this.selectedChat)!.find(m => m.id == id)!
      reply.innerHTML = 'Reply ' + (this.replyIsPersonal ? 'personally ' : '') +  'to: <em>' + message.username
        + ': ' + message.text + '</em>'
      reply.style.visibility = 'visible'
      document.getElementById('cancel-reply')!.style.visibility = 'visible'
      this.replyTo = id
    }
    document.getElementById('reply-in-group')!.addEventListener('click', () => {
      this.replyIsPersonal = false
      callback()
    })
    document.getElementById('reply-personally')!.addEventListener('click', () => {
      this.replyIsPersonal = true
      callback()
    })
    replyDialog.showModal()
  }

  public cancelReply() {
    this.replyTo = -1
    document.getElementById('reply')!.style.visibility = 'hidden'
    document.getElementById('cancel-reply')!.style.visibility = 'hidden'
  }

  public findMessage(replyTo: number)  {
    return this.signalrService.data.get(this.selectedChat)!.find(m => m.id == replyTo)
  }

  getDate(message: Message) {
    let dateString = message.date
    if (!dateString.endsWith(' UTC')) {
      dateString += ' UTC'
    }
    return new Date(dateString).toLocaleString()
  }

  public repliedToMe(message: Message): boolean {
    let username = this.storageService.getToken().username
    if (message.username === username) {
      return false
    }

    let replyMessage = this.signalrService.data.get(this.selectedChat)!.find(m => m.id === message.replyTo)
    if (replyMessage === undefined) {
      return false
    }

    return replyMessage.username === username;
  }
}
