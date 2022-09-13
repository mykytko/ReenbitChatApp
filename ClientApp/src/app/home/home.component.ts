import {Component, OnInit} from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  blocks: Block[] = [{ chatName: 'gaymers', lastMessageSender: 'mykytko',
    lastMessageText: 'hello gays', isNew: true},
    { chatName: 'non-gaymers', lastMessageSender: 'oleh',
      lastMessageText: 'hello straights and everyone who is not gay;', isNew: false}];
  selectedChat: string = "";

  ngOnInit() {
    // do SignalR magic
  }

  openChat(chatName: string) {
    this.selectedChat = chatName;
    // display chat's messages and prompt
  }


}

class Block {
  chatName: string = "";
  lastMessageSender: string = "";
  lastMessageText: string = "";
  isNew: boolean = false;
}
