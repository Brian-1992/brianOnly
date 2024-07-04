Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "寄售存放單位維護";
    var T1GetExcel = '/api/AA0188/Excel';
    var amtGet = '/api/AA0188/calcAmtMsg';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var whnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0188/GetWhnoCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var storeItems = data.etts;
                    if (storeItems.length > 0) {
                        for (var i = 0; i < storeItems.length; i++) {
                            whnoStore.add({ VALUE: storeItems[i].VALUE, TEXT: storeItems[i].TEXT, COMBITEM: storeItems[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P0',
        name: 'P0',
        fieldLabel: '藥材代碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 170,
        matchFieldWidth: false,
        listConfig: { width: 200 },
        limit: 20, //限制一次最多顯示20筆
        queryUrl: '/api/AA0188/GetMMCODECombo' //指定查詢的Controller路徑
    });

    var T1FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 200,
        limit: 20, //限制一次最多顯示20筆
        queryUrl: '/api/AA0188/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'DISC_CPRICE'], //查詢完會回傳的欄位
        allowBlank: false,
        fieldCls: 'required',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T1Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T1Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T1Form.getForm().findField('DISC_CPRICE').setValue(r.get('DISC_CPRICE'));
            }
        },
    });
    
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                T1QuryMMCode,
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'label',
                    text: '',
                    padding: '0 4 0 16',
                    name: 'T1Query_AmtMsg',
                    id: 'T1Query_AmtMsg',
                    width: 500
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'WH_NAME', 'MMCODE', 'MMNAME_C ', 'MMNAME_E', 'QTY', 'RQTY', 'DISC_CPRICE']
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().getValues()['P0']
                };
                Ext.apply(store.proxy.extraParams, np);
                T1Query.down('#T1Query_AmtMsg').setText('');
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1Tool.down('#export').setDisabled(false);
                    }
                    else {
                        T1Tool.down('#export').setDisabled(true);
                        msglabel('查無資料');
                    }

                    calAmtMsg();
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0188/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AA0188/Create';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0188/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1delete', text: '刪除', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0188/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            {
                itemId: 'export', id: 'export', text: '匯出', disabled: true, handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'MMCODE', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            f.findField('MMCODE').setReadOnly(false);
            f.findField('WH_NO').setReadOnly(false);
            f.findField('WH_NO').setValue('');
            f.findField('QTY').setValue('1');
        }
        f.findField('x').setValue(x);
        f.findField('QTY').setReadOnly(false);

        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "藥材代碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "寄放單位",
            dataIndex: 'WH_NAME',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "數量",
            dataIndex: 'QTY',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "單價",
            dataIndex: 'DISC_CPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#t1edit').setDisabled(T1Rec == 0);
        T1Grid.down('#t1delete').setDisabled(T1Rec == 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('MMCODE').setReadOnly(true);
            f.findField('WH_NO').setReadOnly(true);
            f.findField('QTY').setReadOnly(true);

            T1Grid.down('#t1edit').setDisabled(false);
            T1Grid.down('#t1delete').setDisabled(false);
        }
        else {
            T1Form.getForm().reset();

        }

    }

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden',
            width: 220
        },
            T1FormMMCode,
        {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            readOnly: true,
            name: 'MMNAME_C'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            readOnly: true,
            name: 'MMNAME_E'
        }, {
            xtype: 'combo',
            fieldLabel: '寄放單位',
            store: whnoStore,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            name: 'WH_NO',
            fieldCls: 'required',
            allowblank: false,
            anyMatch: true
        }, {
            xtype: 'numberfield',
            fieldLabel: '數量',
            name: 'QTY',
            minValue: 1,
            allowDecimals: false, //是否允許輸入小數
            keyNavEnabled: false,
            mouseWheelEnabled: false,
            fieldCls: 'required',
            allowBlank: false
        }, {
            xtype: 'displayfield',
            fieldLabel: '單價',
            readOnly: true,
            name: 'DISC_CPRICE'
        }],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
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
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            calAmtMsg();
                            break;
                    }
                    T1Cleanup();
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
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
    }

    function calAmtMsg() {
        // 統計寄售總量
        Ext.Ajax.request({
            url: amtGet,
            method: reqVal_p,
            params: {
                MMCODE: T1Query.getForm().findField('P0').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg != '') {
                        T1Query.down('#T1Query_AmtMsg').setText(data.msg);
                    }
                } else {
                    T1Query.down('#T1Query_AmtMsg').setText('');
                }
            },
            failure: function (response, options) {
            }
        });

    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tGrid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [{
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
                            itemId: 't1Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            items: [T1Grid]

                        }
                    ]
                }]
            },
            {
                itemId: 'form',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
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
    T1Query.getForm().findField('P0').focus();
});
