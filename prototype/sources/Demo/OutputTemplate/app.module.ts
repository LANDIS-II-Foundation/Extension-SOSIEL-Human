import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ResultsTable } from 'components/results.table.js'
import { HttpModule } from '@angular/http';

@NgModule({
    imports: [BrowserModule, HttpModule],
    declarations: [ResultsTable],
    bootstrap: [ResultsTable]
})
export class AppModule {
}