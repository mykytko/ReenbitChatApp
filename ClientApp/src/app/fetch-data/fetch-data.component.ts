import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public forecasts: Message[] = [];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    console.log(baseUrl + 'message/get')
    http.get<Message[]>(baseUrl + 'message/get').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));
  }
}

interface Message {
  messageId: number;
  userId: number;
  text: string;
  dateTime: Date;
  chatId: number;
}
