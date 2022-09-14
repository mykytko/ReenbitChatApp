import {Inject, Injectable} from "@angular/core";
import * as signalR from "@microsoft/signalr";
import {StorageService} from "./storage.service";

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private url: string;
  public data: Message[] = [];

  private hubConnection: signalR.HubConnection;
  constructor(@Inject('BASE_URL') baseUrl: string, private storageService: StorageService) {
    this.url = baseUrl + 'chat';
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.url)
      .build();
    this.hubConnection
      .start()
      .then(() => console.log('connection initiated'))
      .catch(err => console.log('error while attempting to initiate connection: ' + err));
  }

  public addBroadcastMessagesListener() {
    this.hubConnection.on('BroadcastMessages', data => {
      this.data = JSON.parse(data);
      console.log(data);
    });
  }

  public addBroadcastMessageListener() {
    this.hubConnection.on('BroadcastMessage', data => {
      this.data.push(JSON.parse(data));
      console.log(data);
    })
  }

  public requestMessages(chat: string) {
    this.hubConnection.invoke('BroadcastMessages',
      this.getConnectionId(), 0, chat).catch(err => console.log(err));
  }

  public getConnectionId() {
    return this.hubConnection.connectionId;
  }

  public sendMessage(chat: string, messageText: string) {
    let username = this.storageService.getToken().username;
    this.hubConnection.invoke('BroadcastMessage', chat, username, messageText).catch(err => console.log(err));
  }
}

interface Message {
  username: string;
  text: string;
  date: string;
}
