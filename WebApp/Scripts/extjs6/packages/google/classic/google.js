Ext.define('Ext.google.ux.Client',{extend:'Ext.Mixin',mixins:['Ext.mixin.Mashup'],requiredScripts:['//apis.google.com/js/client.js?onload=_ext_google_ux_client_initialize_'],statics:{getApiVersion:function(b){var a=this.libraries[b];return a&&a.state==2?a.version:null}},mixinConfig:{extended:function(c,b,a){this.load(a.googleApis)}},onClassMixedIn:function(a){this.load(a.prototype.googleApis)},privates:{statics:{initialized:!1,blocked:!1,loading:0,libraries:{},load:function(d){var c=this.libraries,b,a;if(!Ext.isObject(d)){return}Ext.Object.each(d,function(e,f){b=f.version||'v1';a=c[e];if(!Ext.isDefined(a)){c[e]={version:b,state:0}}else if(a.version!==b){Ext.log.error('Google API: failed to load version "'+b+'" of the','"'+e+'" API: "'+a.version+'" already loaded.')}});this.refresh()},refresh:function(){var a=this;if(!a.initialized){return}if(!a.blocked){Ext.env.Ready.block();a.blocked=!0}Ext.Object.each(a.libraries,function(c,b){if(b.state==0){b.state=1;gapi.client.load(c,b.version,function(){b.state=2;if(!--a.loading){a.refresh()}})}if(b.state==1){a.loading++}});if(!a.loading&&a.blocked){Ext.env.Ready.unblock();a.blocked=!1}},initialize:function(){this.initialized=!0;this.refresh()}}}});_ext_google_ux_client_initialize_=function(){gapi.auth.init(function(){Ext.google.ux.Client.initialize()})};Ext.define('Ext.google.data.AbstractProxy',{extend:'Ext.data.proxy.Server',mixins:['Ext.google.ux.Client'],batchActions:!1,reader:{type:'json',rootProperty:'items',messageProperty:'error'},doRequest:function(c){var b=this,a=b.buildRequest(c),d=b.getWriter(),e=!1;if(d&&c.allowWrite()){a=d.write(a)}b.execute(b.buildApiRequests(a)).then(function(d){b.processApiResponse(c,a,d)});return a},buildUrl:function(a){return ''},privates:{execute:function(a){a=[].concat(a);var b=[];return Ext.Array.reduce(a,function(c,d){return c.then(function(){return d.then(function(e){b.push(e)})})},Ext.Deferred.resolved()).then(function(){return {result:b}})},processApiResponse:function(d,e,b){var a=!1,c=[];Ext.each(Object.keys(b.result),function(g){var f=b.result[g].result;if(f.error){a=f.error.message;return !1}c.push(f)});this.processResponse(!0,d,e,{results:a?[]:c,success:!a,error:a})},sanitizeItems:function(c){var a=[],b=[];Ext.Array.each(c,function(d){if(!Ext.Array.contains(b,d.id)){a.push(d);b.push(d.id)}},this,!0);return a}}});Ext.define('Ext.google.data.EventsProxy',{extend:'Ext.google.data.AbstractProxy',alias:'proxy.google-events',googleApis:{'calendar':{version:'v3'}},buildApiRequests:function(a){var b=this,c=a.getAction();switch(c){case 'read':return b.buildReadApiRequests(a);case 'create':return b.buildCreateApiRequests(a);case 'update':return b.buildUpdateApiRequests(a);case 'destroy':return b.buildDestroyApiRequests(a);default:Ext.raise('unsupported request: events.'+c);return null;}},extractResponseData:function(d){var a=this,c=a.callParent(arguments),b=[];Ext.each(c.results,function(c){switch(c.kind){case 'calendar#events':b=b.concat(c.items.map(a.fromApiEvent.bind(a)));break;case 'calendar#event':b.push(a.fromApiEvent(c));break;default:break;}});return {items:a.sanitizeItems(b),success:c.success,error:c.error}},privates:{toApiEvent:function(c,b){var a={};Ext.Object.each(c,function(f,e){var g=null,d=null;switch(f){case 'calendarId':case 'description':a[f]=e;break;case 'id':a.eventId=e;break;case 'title':a.summary=e;break;case 'startDate':case 'endDate':if(b){d=new Date(e);d.setHours(0,-d.getTimezoneOffset());d=Ext.Date.format(d,'Y-m-d')}else {g=Ext.Date.format(new Date(e),'c')};a[f.slice(0,-4)]={date:d,dateTime:g};break;default:break;}});return a},fromApiEvent:function(b){var a={allDay:!0};Ext.Object.each(b,function(g,c){var d,f,e;switch(g){case 'id':case 'description':a[g]=c;break;case 'summary':a.title=c;break;case 'start':case 'end':d=Ext.Date.parse(c.dateTime||c.date,'C');f=d.getTimezoneOffset();e=!!c.date;if(e&&f!==0){d.setHours(0,-f)};a[g+'Date']=d;a.allDay=a.allDay&&e;break;default:break;}});return a},buildReadApiRequests:function(f){var c=f.getParams(),b=new Date(c.startDate),d=new Date(c.endDate),e=[],a;while(b<d){a=Ext.Date.add(b,Ext.Date.MONTH,3);if(a>d){a=d}e.push(gapi.client.calendar.events.list({calendarId:c.calendar,timeMin:Ext.Date.format(b,'C'),timeMax:Ext.Date.format(a,'C'),singleEvents:!0,maxResults:2500}));b=a}return e},buildCreateApiRequests:function(a){var b=a.getRecords()[0];return gapi.client.calendar.events.insert(this.toApiEvent(a.getJsonData(),b.get('allDay')))},buildUpdateApiRequests:function(g){var b=g.getRecords()[0],a=this.toApiEvent(g.getJsonData(),b.get('allDay')),d=b.getModified('calendarId'),c=b.get('calendarId'),f=b.getId(),e=[];a.calendarId=c;a.eventId=f;if(d&&d!==c){e.push(gapi.client.calendar.events.move({destination:c,calendarId:d,eventId:f}))}if(Object.keys(a).length>2){e.push(gapi.client.calendar.events.patch(a))}return e},buildDestroyApiRequests:function(a){var b=a.getRecords()[0];data=a.getJsonData();data.calendarId=data.calendarId||b.get('calendarId')||b.getPrevious('calendarId');return gapi.client.calendar.events['delete']({'calendarId':data.calendarId,'eventId':data.id})}}});Ext.define('Ext.google.data.CalendarsProxy',{extend:'Ext.google.data.AbstractProxy',alias:'proxy.google-calendars',requires:['Ext.google.data.EventsProxy'],googleApis:{'calendar':{version:'v3'}},buildApiRequests:function(a){var c=this,b=a.getAction();switch(b){case 'read':return c.buildReadApiRequests(a);case 'update':return c.buildUpdateApiRequests(a);default:Ext.raise('unsupported request: calendars.'+b);return null;}},extractResponseData:function(d){var a=this,c=a.callParent(arguments),b=[];Ext.each(c.results,function(c){switch(c.kind){case 'calendar#calendarList':b=b.concat(c.items.map(a.fromApiCalendar.bind(a)));break;default:break;}});return {items:a.sanitizeItems(b),success:c.success,error:c.error}},privates:{toApiCalendar:function(b){var a={};Ext.Object.each(b,function(d,c){switch(d){case 'id':a.calendarId=c;break;case 'hidden':a.selected=!c;break;default:break;}});return a},fromApiCalendar:function(b){var a={hidden:!b.selected,editable:!1,eventStore:{autoSync:!0,proxy:{type:'google-events',resourceTypes:'events'}}};Ext.Object.each(b,function(d,c){switch(d){case 'id':case 'description':a[d]=c;break;case 'backgroundColor':a.color=c;break;case 'summary':a.title=c;break;case 'accessRole':a.editable=(c=='owner'||c=='writer');break;default:break;}});return a},buildReadApiRequests:function(a){return gapi.client.calendar.calendarList.list()},buildUpdateApiRequests:function(a){var b=this.toApiCalendar(a.getJsonData());return gapi.client.calendar.calendarList.patch(b)}}});Ext.define('Ext.ux.google.Api',{mixins:['Ext.mixin.Mashup'],requiredScripts:['//www.google.com/jsapi'],statics:{loadedModules:{}},onClassExtended:function(e,d,a){var b=a.onBeforeCreated,c=this;a.onBeforeCreated=function(o,k){var p=this,j=[],l=Ext.Array.from(k.requiresGoogle),h=c.loadedModules,i=0,m=function(){if(!--i){b.call(p,o,k,a)}Ext.env.Ready.unblock()},f,g,n;n=l.length;for(g=0;g<n;++g){if(Ext.isString(f=l[g])){j.push({api:f})}else if(Ext.isObject(f)){j.push(Ext.apply({},f))}}Ext.each(j,function(c){var f=c.api,g=String(c.version||'1.x'),b=h[f];if(!b){++i;Ext.env.Ready.block();h[f]=b=[m].concat(c.callback||[]);delete c.api;delete c.version;if(!window.google){Ext.raise("'google' is not defined.");return !1}google.load(f,g,Ext.applyIf({callback:function(){h[f]=!0;for(var g=b.length;g-->0;){b[g]()}}},c))}else if(b!==!0){b.push(m)}});if(!i){b.call(p,o,k,a)}}}});