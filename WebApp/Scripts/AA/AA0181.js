Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "合約優惠設定作業";
    var currentDataYm = '';
    var dataYmGet = '/api/AA0181/getDataYm';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' }
        ]

    });

    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'vbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,
        items: [
            {
                xtype: 'container',
                layout: 'hbox',
                items: [
                    {
                        xtype: 'panel',
                        id: 'PanelP1',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'monthfield',
                                fieldLabel: '查詢月份',
                                name: 'P0',
                                id: 'P0',
                                labelWidth: 80,
                                width: 180,
                                fieldCls: 'required',
                                //value: new Date(),
                                allowBlank: false
                            },
                            {
                                xtype: 'textfield',
                                fieldLabel: '案號',
                                name: 'P1',
                                id: 'P1',
                                labelWidth: 80,
                                width: 180
                            },
                            {
                                xtype: 'button',
                                text: '查詢',
                                handler: function () {
                                    T1Load();
                                }
                            },
                            {
                                xtype: 'button',
                                text: '清除',
                                handler: function () {
                                    var f = this.up('form').getForm();
                                    f.reset();
                                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                }
                            },
                            {
                                xtype: 'button',
                                text: '匯出', handler: function () {
                                    var p = new Array();
                                    var p0 = T1Query.getForm().findField('P0').rawValue;
                                    var p1 = T1Query.getForm().findField('P1').rawValue;
                                    var url =   '../../../api/AA0181/Excel';
                                    p.push({ name: 'p0', value: p0 });
                                    p.push({ name: 'p1', value: p1 });
                                    PostForm(url, p);
                                    msglabel('訊息區:匯出完成');
                                }
                            }
                        ]
                    }
                ]
            }

        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F2', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0181/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        Ext.getCmp('btnUpdate').setDisabled(true);
        Ext.getCmp('btnDel').setDisabled(true);
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                //disabled: true,
                handler: function () {
                    T1Set = '/api/AA0181/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                disabled: true,
                handler: function () {
                    T1Set = '/api/AA0181/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let caseno = '';

                        $.map(selection, function (item, key) {
                            name += '「' + item.get('F1') + '」<br>';
                            caseno += item.get('F1') + ',';
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除合約案號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0181/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        CASENO: caseno,
                                        DATA_YM: T1Query.getForm().findField('P0').rawValue
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            T1Grid.getSelectionModel().deselectAll();

                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);

                                            T1Cleanup();
                                            T1Load();
                                        }
                                        else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                            msglabel('訊息區:' + data.msg);
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
        ]
    });

    function T1ChkBtns() {
        var selection = T1Grid.getSelection();
        if (selection.length) {
            if (selection.length == 1) {
                // 資料年月與開帳年月不同則不可修改
                if (selection[0].data.DATA_YM == currentDataYm)
                    Ext.getCmp('btnUpdate').setDisabled(false);
                else
                    Ext.getCmp('btnUpdate').setDisabled(true);
            }
            else {
                Ext.getCmp('btnUpdate').setDisabled(true);
            }
            Ext.getCmp('btnDel').setDisabled(false);
        }
        else {
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('btnDel').setDisabled(true);
        }
    }

    var checkboxT1Model = Ext.create('Ext.selection.CheckboxModel', {
        listeners: {
            'beforeselect': function (view, rec, index) {
            },
            'select': function (view, rec) {
                T1ChkBtns();
            },
            'deselect': function (view, rec) {
                T1ChkBtns();
            }
        }
    });

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {

        if (T1LastRec != null) {
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            f.findField("CASENO").setValue(T1LastRec.data["F1"]);
            f.findField("CASENO_TEXT").setValue(T1LastRec.data["F1"]);
            f.findField("JBID_RCRATE").setValue(T1LastRec.data["F2"]);
            f.findField("JBID_RCRATE_TEXT").setValue(T1LastRec.data["F2"]);
            f.findField("DATAYM").setValue(T1Query.getForm().findField('P0').rawValue); //載入資料年月

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            f.findField("DATAYM").setValue(currentDataYm); //新增之預設值為開帳年月
            setCmpShowCondition(true, true);
        } else {
            f.findField("DATAYM").setValue(T1Query.getForm().findField('P0').rawValue); //載入資料年月
            setCmpShowCondition(false, true);
        }

        f.findField('x').setValue(x);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
    }

    function showComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.show();
    }

    function hideComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.hide();
    }

    //控制不可更改項目的顯示
    function setCmpShowCondition(F1, F2) {
        var f = T1Form.getForm();
        F1 ? showComponent(f, "CASENO") : hideComponent(f, "CASENO");
        !F1 ? showComponent(f, "CASENO_TEXT") : hideComponent(f, "CASENO_TEXT");
        F2 ? showComponent(f, "JBID_RCRATE") : hideComponent(f, "JBID_RCRATE");
        !F2 ? showComponent(f, "JBID_RCRATE_TEXT") : hideComponent(f, "JBID_RCRATE_TEXT");
    }

    // 顯示明細/新增/修改輸入欄
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
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '資料年月',
                name: 'DATAYM'
            },
            {
                fieldLabel: '合約案號',
                name: 'CASENO',
                allowBlank: false,
                fieldCls: 'required',
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '合約案號',
                name: 'CASENO_TEXT'
            },
            {
                fieldLabel: '管理費%',
                name: 'JBID_RCRATE',
                allowBlank: false,
                fieldCls: 'required',
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '管理費%',
                name: 'JBID_RCRATE_TEXT'
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
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
                    var msg = action.result.msg;

                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var TQ = T1Query.getForm();
                            T1Load();
                            msglabel(msg);

                            Ext.Msg.alert('訊息', msg);
                            break;
                        case "U":
                            msglabel('訊息區:資料更新成功');
                            T1Load();
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

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        selModel: checkboxT1Model,
        columns: [
            {
                //xtype: 'rownumberer',
                text: "項次",
                dataIndex: 'rnm',
                width: 60
            },
            {
                text: "合約案號",
                dataIndex: 'F1',
                width: 100
            },
            {
                text: "管理費%",
                dataIndex: 'F2',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                msglabel('訊息區:');

                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setCmpShowCondition(false, false);
        checkboxT1Model.deselectAll();
    }

    function getCurrentDataYm() {
        Ext.Ajax.request({
            url: dataYmGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg != '') {
                        currentDataYm = data.msg;
                        var currentY = currentDataYm.substr(0, 3);
                        var currentM = currentDataYm.substr(3, 2);
                        var currentYm = Ext.Date.parse((parseInt(currentY) + 1911).toString()+currentM, 'Ym'); //預設為月結已開帳時間
                        T1Query.getForm().findField('P0').setValue(currentYm);
                    } else {
                        T1Query.getForm().findField('P0').setValue(new Date());
                    }
                }
            },
            failure: function (response, options) {
            }
        });

    }
    getCurrentDataYm();

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            id: 'eastform',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }]
    });

    setCmpShowCondition(false, false);
});