Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);



Ext.onReady(function () {
    //var T1Get = '../../../api/QUOTN/QueryNO';
    //var T1GetExcel = '../../../api/QUOTN/Excel2';
    var T1Name = "藥局領用記錄";
    var T1Rec = 0;
    var T1LastRec = null;
   
    var pSize = 20; //分頁時每頁筆數


    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['DOCNO', 'SEQ', 'CHECKSEQ', 'GENWAY', 'ACKQTY', 'ACKTIME', 'ACKID']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });



    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));//取得傳過來的參數值
        return param ? params[param] : params;
    };

    var s = Ext.getUrlParam('strParam');;
    //var ss = s.split(",");
   // alert(s)
    var DOCNO = s
    
    //alert(DOCNO)

    var mLabelWidth = 90;
    var mWidth = 210;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
    });

   

   
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: DOCNO,
                   
                };      
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url:'/api/AB0018/QueryMEDOCC',
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

   

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        //plugins: [{
        //    ptype: "pagesize",
        //    pageSize: pSize
        //}],
        
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {
            beforechange: function (T1Tool, pageData) {
                T1Rec = 0; //disable編修按鈕&刪除按鈕
                T1LastRec = null; //T1Form之資料輸選區清空
            },
            afterrender: function (T1Tool) {
                T1Tool.emptyMsg = '<font color=red>沒有任何資料</font>';
            }
        },
      
    });


    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        menuDisabled: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "單據號碼",
            dataIndex: 'DOCNO',
            align: 'left',
            menuDisabled: true,
            style: 'text-align:left',
            width: 120
            }, {
           // ['', '', '', 'UNIT', '', '', '', 'Q&A', '', 'RFQ_DEADLINE', 'Quh', '', '']
            text: "項次",
            dataIndex: 'SEQ',
            menuDisabled: true,
            align: 'left',
            style: 'text-align:left',
            width: 40
        }, {
            text: "點收次數",
            dataIndex: 'CHECKSEQ',
            menuDisabled: true,
            align: 'left',
            style: 'text-align:left',
            width: 120
        }, {
            text: "產生方式",
            dataIndex: 'GENWAY',
            menuDisabled: true,
            align: 'left',
            style: 'text-align:left',
            width: 110
        }, {
            text: "點收數量",
            dataIndex: 'ACKQTY',
            menuDisabled: true,
            align: 'right',
            style: 'text-align:left',
            width: 100
        }, {
            text: "點收日期",
            dataIndex: 'ACKTIME',
            menuDisabled: true,
            align: 'left',
            style: 'text-align:left',
            width: 120
        }, {
            text: "點收人員",
            dataIndex: 'ACKID',
            menuDisabled: true,
            align: 'left',
            style: 'text-align:left',
            sort: false,
            width: 100

         }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    var viewport = Ext.create('Ext.Viewport', {
        id: 'viewport',
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });
    //var uid = parent.parent.userId
    //msglabel("");
    T1Load();

});