Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "公藥補撥確認作業";
    var T1Rec = 0;
    var T1LastRec = null;
    //var T1Name = "";

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //function getToday() {
    //    var today = new Date();
    //    today.setDate(today.getDate());
    //    return today
    //}

    var inid_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0193/GetInidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, autoLoad: true
    });

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('ACKQTY').setReadOnly(true);
        f.findField('ISWAS').setReadOnly(true);
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').collapse();

        var selectionModel = T2Grid.getSelectionModel();
        selectionModel.deselectAll();
    }

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        //cls: 'T1b',
        title: '',
        autoScroll: true,
        //bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 110,
            width: 200,
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 2,
                },
                items: [
                    {
                        name: 'x',
                        xtype: 'hidden'
                    }, {
                        name: 'DOCNO',
                        submitValue: true,
                        xtype: 'hidden'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '藥基準量',
                        name: 'HIGH_QTY',
                        readOnly: true,
                        //submitValue: true
                    },{
                        xtype: 'displayfield',
                        fieldLabel: '藥材代碼',
                        name: 'MMCODE',
                        readOnly: true,
                        submitValue: true
                    }, {
                        xtype: 'combo',
                        fieldLabel: '列為消耗',
                        name: 'ISWAS',
                        id: 'ISWAS_combo',
                        store: Ext.create('Ext.data.Store', {
                            fields: ['value', 'displayText'],
                            data: [
                                { value: 'Y', displayText: '是' },
                                { value: 'N', displayText: '否' }
                            ]
                        }),
                        queryMode: 'local',
                        displayField: 'displayText',
                        valueField: 'value',
                        editable: false,
                        readOnly: true,
                        fieldCls: 'required',
                        submitValue: true
                    },{
                        xtype: 'displayfield',
                        fieldLabel: '藥材名稱',
                        name: 'MCODE_NAME',
                        readOnly: true,
                        //submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '定時補撥',
                        name: 'ISDEF', 
                        readOnly: true,
                        //submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '單位',
                        name: 'BASE_UNIT',
                        readOnly: true,
                        //submitValue: true
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '補發數量',
                        name: 'ACKQTY',
                        fieldCls: 'required',
                        readOnly: true,
                        submitValue: true,
                        minValue: 0,
                        allowBlank: false,
                        decimalPrecision: 0
                    }
                ]
            }
        ],
        buttons: [
            {
                id: 'submit', itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (T1Form.getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit;//= viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm('提示', '是否確定儲存?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                                isNew = false;
                            }
                        }
                        );
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel('訊息區:輸入資料格式有誤');
                    }
                }
            }, {
                id: 'cancel', itemId: 'cancel', text: '取消', hidden: true, handler: function () {
                    T1Cleanup();
                    isNew = false;
                }
            },
        ]
    });
    //Ext.define('T1Model', {
    //    extend: 'Ext.data.Model',
    //    fields: [
    //        { name: 'DOCNO', type: 'string' },
    //        { name: 'APPTIME', type: 'string' },
    //        { name: 'APP_INID', type: 'string' },
    //        { name: 'APP_INID_N', type: 'string' },
    //        { name: 'APPDEPT_N', type: 'string' }
    //    ]
    //});
    var T1Store = Ext.create('Ext.data.Store', {
        //model: 'T1Model',
        pageSize: 5, // 每頁顯示筆數
        remoteSort: true,
        sorters: [ { property: 'DOCNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0193/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        Ext.getCmp('btnT1edit').setDisabled(true);
        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    var T2Rec = 0;
    var T2LastRec = null;

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [{
                xtype: 'combo',
                fieldLabel: '單位區間',
                name: 'P1',
                labelWidth: 60,
                width: 260,
                store: inid_store,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                fieldLabel: '~',
                name: 'P2',
                labelWidth: 20,
                width: 220,
                store: inid_store,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                padding: '0 4 0 4',
                labelSeparator: ''
            }, {
                xtype: 'button',
                text: '查詢',
                handler: T1Load
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus();
                }
            }]
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'apply', text: '確認撥補', disabled: true, handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })
                        Ext.MessageBox.confirm('撥補確認', '補撥單：' + name + '，是否確定核可？', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0193/Apply',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:確認核可成功');
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var data = Ext.decode(action.response.responseText);
                    if (data.success) {
                        if (data.msg.toString() == "") {
                            switch (f2.findField("x").getValue()) {
                                case "U":
                                    T2Load();
                                    msglabel('訊息區:資料修改成功');
                                    isNew = false;
                                    break;
                            }
                            T1Cleanup();
                        } else {
                            Ext.MessageBox.alert('警示', data.msg);
                        }
                    }
                    else
                        Ext.MessageBox.alert('錯誤', data.msg);

                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
   
    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T1Store,
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
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
                text: "補撥單編號",
            dataIndex: 'DOCNO',
            width: 130,
            style: 'text-align:left',
            align: 'center'
        },  {
                text: "補撥單位",
                dataIndex: 'APPDEPT',
            width: 130
        }, {
                text: "補撥單位名稱",
                dataIndex: 'INID_NAME',
            width: 130
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            T1F1 = T1LastRec.data["DOCNO"];
            T1Grid.down('#apply').setDisabled(false);
            viewport.down('#form').collapse();
        } else
            T1F1 = '';
        T1Query.getForm().reset();
        Ext.getCmp('btnT1edit').setDisabled(true);

        T2Load();
    }

    //點選T1Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec) {
            T1Form.loadRecord(T2LastRec);

            if (T2LastRec.data.ISWAS == '是') {
                T1Form.down('#ISWAS_combo').setValue('Y');
            }
            else
            {
                T1Form.down('#ISWAS_combo').setValue('N');
            }

            
            Ext.getCmp('btnT1edit').setDisabled(false);
            Ext.getCmp('submit').hide();
            Ext.getCmp('cancel').hide();
            viewport.down('#form').expand();
        }
        else {
            T1Form.getForm().reset();
        }
    }

    //Ext.define('T2Model', {
    //    extend: 'Ext.data.Model',
    //    fields: [
    //        { name: 'DOCNO', type: 'string' },
    //        { name: 'SEQ', type: 'string' },
    //        { name: 'MMCODE', type: 'string' },      //藥材代碼
    //        { name: 'APPQTY', type: 'string' },      //補藥量
    //        { name: 'APVQTY', type: 'string' },      //核可補藥量
    //        { name: 'INV_QTY', type: 'string' },     //使用單位現存量
    //        { name: 'A_INV_QTY', type: 'string' },  //藥局存量
    //        { name: 'OPER_QTY', type: 'string' },    //基準量
    //        { name: 'BASE_UNIT', type: 'string' },   //藥材單位
    //        { name: 'MMNAME_C', type: 'string' }  //藥材名稱
    //    ]
    //});
    var T2Store = Ext.create('Ext.data.Store', {
        //model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0193/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F1
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    //按下新增(I)/修改(U) btn
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        Ext.getCmp('submit').show();
        Ext.getCmp('cancel').show();
        
        if (T1LastRec) {
            f.findField('ACKQTY').setReadOnly(false);
            f.findField('ISWAS').setReadOnly(false);
        }
        u = f.findField('ACKQTY');

        f.findField('x').setValue(x);
        f.findField('DOCNO').setValue(T1F1);
        
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    function T2Load() {
        try {
            T2Store.load({
                params: {
                    start: 0
                }
            });
            T2Tool.moveFirst();
            var f = T1Form.getForm();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }
   
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '修改',
                name: 'btnT1edit', id: 'btnT1edit',
                disabled: true,
                handler: function () {
                    T1Set = '/api/AA0193/UpdateM';
                    msglabel('訊息區:');
                    if (T1LastRec) {
                        setFormT1("U", '修改');
                    }
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "藥材代碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
                text: "藥材名稱",
            dataIndex: 'MCODE_NAME',
            width: 200,
            sortable: true
        }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
            width: 120,
            sortable: true
        }, {
                text: "補撥數量",
            dataIndex: 'ACKQTY',
            width: 100,
            style: 'text-align:right',
            align: 'right'
        }, {
                text: "藥基準量",
            dataIndex: 'HIGH_QTY',
            width: 100,
            style: 'text-align:right',
            align: 'right'
        }, {
                text: "列為消耗",
            dataIndex: 'ISWAS',
            width: 80
        }, {
                text: "定時補撥",
            dataIndex: 'ISDEF',
            width: 80
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });

    //view 
    var viewport = Ext.create('Ext.Viewport', {
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
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '30%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '70%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        }
            ,
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 420,
            title: '瀏覽',
            border: false,
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    function right(str, num) {
        return str.substring(str.length - num, str.length)
    }
    //T1Load(); // 進入畫面時自動載入一次資料
    //T1QueryForm.getForm().findField('P0').focus();

});
