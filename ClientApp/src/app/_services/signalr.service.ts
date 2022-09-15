import {Inject, Injectable} from "@angular/core"
import * as signalR from "@microsoft/signalr"
import {StorageService} from "./storage.service"

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private url: string
  public data: Message[] = []

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
    await this.hubConnection.start();
  }

  public addBroadcastMessagesListener() {
    if (this.hubConnection === undefined) {
      console.log("error: connection not initiated")
      return
    }
    this.hubConnection.on('BroadcastMessages', data => {
      this.data = JSON.parse(data);
      console.log(data);
    });
  }

  public addBroadcastMessageListener() {
    if (this.hubConnection === undefined) {
      console.log("error: connection not initiated")
      return
    }
    this.hubConnection.on('BroadcastMessage', data => {
      this.data.push(JSON.parse(data));
      console.log(data);
    })
  }

  public requestMessages(chat: string) {
    if (this.hubConnection === undefined) {
      console.log("error: connection not initiated")
      return
    }
    this.hubConnection.invoke('BroadcastMessages',
      this.getConnectionId(), 0, chat).catch(err => console.log(err));
  }

  public getConnectionId() {
    if (this.hubConnection === undefined) {
      console.log("error: connection not initiated")
      return
    }
    return this.hubConnection.connectionId;
  }

  public sendMessage(chat: string, messageText: string) {
    if (this.hubConnection === undefined) {
      console.log("error: connection not initiated")
      return
    }
    this.hubConnection.invoke('BroadcastMessage', chat, messageText).catch(err => console.log(err));
  }
}

interface Message {
  username: string;
  text: string;
  date: string;
}
