import {Inject, Injectable} from "@angular/core";
import * as signalR from "@microsoft/signalr";

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private url: string;
  public data: {} = {};

  private hubConnection: signalR.HubConnection;
  constructor(@Inject('BASE_URL') baseUrl: string) {
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
      this.data = data;
      console.log(data);
    });
  }

  public requestMessages(chat: string) {
    this.hubConnection.invoke('BroadcastMessages',
      this.getConnectionId(), 0, chat).catch(err => console.log(err));
  }

  public getConnectionId() {
    return this.hubConnection.connectionId;
  }
}
