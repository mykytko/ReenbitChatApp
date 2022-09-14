import {Component, Inject, OnInit} from '@angular/core';
import {SignalrService} from "../_services/signalr.service";
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {StorageService} from "../_services/storage.service";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  blocks: Block[] = [{ chatName: 'group chat', lastMessageSender: 'mykytko',
    lastMessageText: 'hello gays', isNew: true},
    { chatName: 'personal chat', lastMessageSender: 'oleh',
      lastMessageText: 'hello straights and everyone who is not gay;', isNew: false}];
  selectedChat: string = "";
  url: string;
  messageText: string = "";

  constructor(public signalrService: SignalrService, private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private storageService: StorageService) {
    this.url = baseUrl;
  }

  ngOnInit() {
    this.signalrService.addBroadcastMessagesListener();
    this.signalrService.addBroadcastMessageListener();

    let sendButton = document.getElementById("message-text");
    sendButton!.addEventListener("keypress", function(event) {
      if (event.key === "Enter") {
        event.preventDefault();
        document.getElementById("send")!.click();
      }
    });
  }

  openChat(chatName: string) {
    this.selectedChat = chatName;
    this.signalrService.requestMessages(chatName);
  }

  sendMessage() {
    if (this.selectedChat === undefined || this.selectedChat === '' || this.messageText === '') {
      return;
    }

    this.signalrService.sendMessage(this.selectedChat, this.messageText);
    this.messageText = '';
  }
}

interface Block {
  chatName: string;
  lastMessageSender: string;
  lastMessageText: string;
  isNew: boolean;
}
