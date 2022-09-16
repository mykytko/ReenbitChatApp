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
  private nextEnterListenerIsSend = false
  private replyTo = -1

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
    this.selectedChat = chatName
    this.signalrService.requestMessages(chatName)
    this.isPersonal = this.signalrService.blocks.find(b => b.chatName == chatName)!.isPersonal
  }

  public sendMessage() {
    if (this.selectedChat === undefined || this.selectedChat === '' || this.messageText === '') {
      return
    }

    this.signalrService.sendMessage(this.selectedChat, this.messageText, this.replyTo)
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
    let reply = document.getElementById('reply')!
    let message = this.signalrService.data.get(this.selectedChat)!.find(m => m.id == id)!
    reply.innerHTML = 'Reply to: <em>' + message.username + ': ' + message.text + '</em>'
    reply.style.visibility = 'visible'
    document.getElementById('cancel-reply')!.style.visibility = 'visible'
    this.replyTo = id
  }

  public cancelReply() {
    this.replyTo = -1
    document.getElementById('reply')!.style.visibility = 'hidden'
    document.getElementById('cancel-reply')!.style.visibility = 'hidden'
  }

  public findMessage(replyTo: number) {
    return this.signalrService.data.get(this.selectedChat)!.find(m => m.id == replyTo)!
  }
}
