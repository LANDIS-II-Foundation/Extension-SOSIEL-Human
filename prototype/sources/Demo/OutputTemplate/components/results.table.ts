import { Component, Injectable, Inject } from '@angular/core';
import { Http } from '@angular/http';
import {Observable} from 'rxjs';
import 'rxjs/Rx';

@Component({
    selector: 'es-results',
    templateUrl: 'components/results.table.template.html'
})
export class ResultsTable {
    data: Array<any> = [];

    constructor(private http: Http) {
        this.http.request('./output.json?ts=' + (new Date()).getTime()).subscribe(res=> this.data = res.json());
    }

}
