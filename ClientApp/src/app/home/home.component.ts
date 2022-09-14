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

  constructor(private signalrService: SignalrService, private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private storageService: StorageService) {
    this.url = baseUrl;
  }

  ngOnInit() {
    this.signalrService.addBroadcastMessagesListener();
  }

  openChat(chatName: string) {
    this.selectedChat = chatName;
    this.signalrService.requestMessages(chatName);
    this.http.get(this.url + 'message/get?connectionId=' + this.signalrService.getConnectionId()
      + '&skip=' + 0 + '&chatName=' + this.selectedChat,
      {
        headers: new HttpHeaders({'Authentication': 'Bearer ' + this.storageService.getToken()})
      }
    );
    // display chat's messages and prompt
  }


}

interface Block {
  chatName: string;
  lastMessageSender: string;
  lastMessageText: string;
  isNew: boolean;
}
