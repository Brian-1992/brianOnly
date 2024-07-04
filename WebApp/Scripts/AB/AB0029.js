Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    //Master

    var T1Set = '';
    var T1Name = "調撥申請";

    //Detail
    var T11Set = '';
    var T11Name = "調撥申請項目";

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var DOCNO = '';
    var DOC_FLOWID = '';
    var WEXP_ID = '';
    var rowData = null;
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    //var isNew = false;
    var action = '';
    var ck0201 = true;
    var ck0202 = true;
    var flowid = '0201';
    var fid = '0201'; // AB0029
    var win1;
    var hosp_code = '';

    function setFlowid() {
        fid = Ext.getUrlParam('fid');
        switch (fid) {
            case "0202": // AB0030
                ck0201 = false;
                ck0202 = true;
                break;
            case "0203": // AB0031
                ck0201 = false;
                ck0202 = false;
                break;
        }
    }
    setFlowid();

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        listeners: {
            load: function (store, eOpts) {
                hosp_code = store.getAt(0).get('HOSP_CODE');
                // 依取得的HOSP_CODE處理欄位
                if ((fid == "0203") && (hosp_code == '803')) {
                    // 1121106配合803曾傳志需求,若為803(台中)則顯示匯出
                    T1Tool.down('#export').setVisible(true);
                }
                else {
                    T1Tool.down('#export').setVisible(false);
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0029/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'LIST_ID', type: 'string' }
        ]
    });
    Ext.define('T12Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'INV_QTY', type: 'float' }
        ]
    });
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'TRNAB_QTY', type: 'string' },
            { name: 'TRNAB_RESON', type: 'string' },
            { name: 'TRNAB_RESON_TEXT', type: 'string' },
            { name: 'INV_QTY', type: 'string' }
        ]
    });
    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: ['EXP_DATE', 'INV_QTY', 'MOVE_QTY']
    });

    var inid_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0029/GetInidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: fid
                };
                Ext.apply(store.proxy.extraParams, np);
            }, load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        }, autoLoad: true
    });
    var qFrwh_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AB0029/GetQFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: fid
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        },
        autoLoad: true
    });
    var towh_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AB0029/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: fid
                };
                Ext.apply(store.proxy.extraParams, np);
            }/*,
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }*/
        },
        autoLoad: true
    });

    var frwh_store = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                var p0 = T1Form.getForm().findField('TOWH').getValue();
                var np = {
                    action: action,
                    p0: p0
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0029/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, autoLoad: true
    });

    var trnab_reson_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0029/GetTrnabResonCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, autoLoad: true
    });

    var T1Query = Ext.widget({
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
                xtype: 'textfield',
                id: 'P0',
                fieldLabel: '申請單號'
            }, {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D1',
                id: 'D1',
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'D2',
                id: 'D2'
            }, {
                xtype: 'textfield',
                id: 'P4',
                fieldLabel: '申請人員',
                labelAlign: 'right'
            }, {
                xtype: 'combo',
                id: 'P5',
                fieldLabel: '申請部門',
                labelAlign: 'right',
                store: inid_store,
                queryMode: 'local',
                matchFieldWidth: false,
                listConfig: {
                    width: 230
                },
                colspan: 2,
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '調出庫房',
                id: 'P3',
                queryMode: 'local',
                store: qFrwh_store,
                displayField: 'TEXT',
                valueField: 'VALUE',
                matchFieldWidth: false,
                listConfig: {
                    width: 230
                }
            }, {
                xtype: 'combo',
                fieldLabel: '調入庫房',
                id: 'P6',
                queryMode: 'local',
                matchFieldWidth: false,
                listConfig: {
                    width: 230
                },
                store: towh_store,
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, {
                xtype: 'checkboxgroup',
                //rowspan: 2,
                columns: 2,
                id: 'P7',
                items: [
                    { boxLabel: '申請中', name: 'P7', inputValue: '0201', checked: ck0201 },
                    { boxLabel: '調出中', name: 'P7', inputValue: '0202', checked: ck0202 },
                    { boxLabel: '調入中', name: 'P7', inputValue: '0203', checked: true },
                    { boxLabel: '已結案', name: 'P7', inputValue: '0299' }
                ],
                getValue: function () {
                    var val = [];
                    this.items.each(function (item) {
                        if (item.checked)
                            val.push(item.inputValue);
                    });
                    return val.toString();
                }
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: T1Load
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0029/AllM',
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
                    fid: fid,
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: Ext.util.Format.date(T1Query.getForm().findField('D1').getValue(), 'Ymd'),
                    p2: Ext.util.Format.date(T1Query.getForm().findField('D2').getValue(), 'Ymd'),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T12Load() {
        T12Tool.moveFirst();
    }
    var T12Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50,
            style: 'text-align:center'
        },
        border: false,
        items: [{
            xtype: 'textfield',
            name: 'P0',
            fieldLabel: '院內碼',
            labelAlign: 'right',
            enforceMaxLength: true,
            maxLength: 13,
            labelWidth: 60,
            width: 200,
        }, {
            xtype: 'textfield',
            name: 'P1',
            fieldLabel: '中文品名',
            labelAlign: 'right',
            labelWidth: 60,
            width: 200,
        }, {
            xtype: 'textfield',
            name: 'P2',
            fieldLabel: '英文品名',
            labelAlign: 'right',
            labelWidth: 60,
            width: 200,
        }, {
            xtype: 'button',
            text: '查詢',
            iconCls: 'TRASearch',
            handler: T12Load
        }, {
            xtype: 'button',
            text: '清除',
            iconCls: 'TRAClear',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }
        ]
    });
    var T12Store = Ext.create('Ext.data.Store', {
        model: 'T12Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'INV_QTY', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0029/GetAllWH',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T12Query.getForm().findField('P0').getValue(),
                    p1: T12Query.getForm().findField('P1').getValue(),
                    p2: T12Query.getForm().findField('P2').getValue(),
                    wh_no: T1Form.getForm().findField('TOWH').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var T12Tool = Ext.create('Ext.PagingToolbar', {
        store: T12Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T12Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T12Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T12Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T12Tool]
            }
        ],
        columns: [{ xtype: 'rownumberer' }, {
            text: "庫房",
            dataIndex: 'WH_NAME',
            width: 200
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "庫存量",
            dataIndex: 'INV_QTY',
            width: 90
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            flex: 0.5
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            flex: 0.5
        }]
    });
    //var T3Store = Ext.create('Ext.data.Store', {
    //    model: 'T3Model',
    //    sorters: [{ property: 'EXP_DATE', direction: 'ASC' }],
    //    proxy: {
    //        type: 'ajax',
    //        actionMethods: {
    //            read: 'POST'
    //        },
    //        url: '/api/AB0029/GetAllWexp',
    //        reader: {
    //            type: 'json',
    //            rootProperty: 'etts',
    //            totalProperty: 'rc'
    //        }
    //    },
    //    listeners: {
    //        beforeload: function (store, options) {
    //            var np = {
    //                p0: FRWH_NO,
    //                p1: T2Form.getForm().findField('MMCODE').getValue()
    //            };
    //            Ext.apply(store.proxy.extraParams, np);
    //        }
    //    }
    //});

    function T1Load() {
        //T1Store.loadPage(1);
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'APPID',
            xtype: 'hidden'
        }, {
            name: 'APPDEPT',
            xtype: 'hidden'
        }, {
            name: 'FLOWID',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'WH_KIND',
            xtype: 'hidden'
        }, {
            name: 'LIST_ID',
            xtype: 'hidden'
        }, {
            name: 'UP',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請單號',
            name: 'DOCNO_D'
        }, {
            xtype: 'combo',
            fieldLabel: '調入庫房',
            name: 'TOWH',
            id: 'TOWH',
            store: towh_store,
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            fieldCls: 'required',
            //anyMatch: true,
            readOnly: true,
            forceSelection: true,
            matchFieldWidth: false,
            listConfig: {
                width: 230
            },
            listeners: {
                select: function (combo, record, index) {
                    //Ext.getCmp('FRWH').setValue("");
                    //Ext.getCmp('FRWH').setRawValue("");
                    frwh_store.load();
                    T12Query.reset();
                    T12Store.removeAll();
                }
            },
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
        }, {
            fieldLabel: '調出庫房',
            xtype: 'combo',
            id: 'FRWH',
            name: 'FRWH',
            store: frwh_store,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            //anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            //triggerAction: 'all',
            matchFieldWidth: false,
            listConfig: {
                width: 230
            },
            fieldCls: 'required',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
        },
        {
            xtype: 'fieldcontainer',
            fieldLabel: '',
            layout: 'hbox',
            items: [{
                xtype: 'button',
                id: 'whBtn',
                iconCls: 'TRASearch',
                handler: function () {
                    showWin1();
                }
            }]
        }
        ],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true, handler: function () {
                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    if (btn === 'yes') {
                        T1Submit();
                    }
                }
                );
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });

    //function T1SelectRow() {
    //    if (rowData != null) {
    //        var index = T1Grid.getStore().findBy(function (record, id) { return record.get('DOCNO') == rowData.DOCNO });
    //        var f2 = T1Form.getForm();
    //        var r = f2.getRecord();
    //        r.set(rowData);
    //        T1Store.insert(0, r);
    //        r.commit();
    //        if (index == -1) {
    //            if (T1Store.length >= T1Store.pageSize)
    //                T1Store.remove(T1Store.pageSize);
    //        } else {
    //            T1Store.remove(index + 1);
    //        }
    //        T1Grid.getView().refresh();
    //        T1Grid.getView().select(0);
    //        rowData = null;
    //    }
    //}

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
                            var v = action.result.etts[0];
                            //r.set(v);
                            T1Query.getForm().findField('P0').setValue(v.DOCNO);
                            //T1Store.insert(0, r);
                            //r.commit();
                            msglabel('資料新增成功');
                            T1Load();
                            T2Store.removeAll();
                            //T1Grid.getView().select(0);
                            //T1Tool.moveFirst();
                            //rowData = action.result.etts[0];
                            action = '';
                            viewport.down('#form').collapse();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('資料修改成功');
                            viewport.down('#form').collapse();
                            action = '';
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('提出申請成功');
                            break;
                        case "B":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('執行調撥成功');
                            break;
                        case "C":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('調撥完成');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('資料刪除成功');
                            break;
                        case "E":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('取消調撥成功');
                            break;
                        case "F":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('資料退回成功');
                            break;
                    }
                    T1Cleanup();
                    //T2Cleanup();
                    TATabs.setActiveTab('Query');
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
        action = '';
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('FRWH').setReadOnly(true);
        f.findField('TOWH').setReadOnly(true);
        T1Form.down('#whBtn').setVisible(false)
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
        T2Cleanup();
        TATabs.setActiveTab('Query');
    }

    function setSubmit(args) {
        T1Form.getForm().findField('UP').setValue(Ext.util.JSON.encode(args.DATA_LIST));
        T1Submit();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {
            change: function (T1Tool, pageData) {
                T1Rec = 0; //disable編修按鈕&刪除按鈕
                T1LastRec = null;
                T1Cleanup();
                // T1SelectRow();
            }
        },
        buttons: [
            {
                itemId: 'add', text: '新增', handler: function () {
                    T1Set = '/api/AB0029/CreateM';
                    setFormT1('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    if (T2Store.data.length == 0) {
                        T1Set = '/api/AB0029/UpdateM';
                        setFormT1("U", '修改');
                    } else {
                        Ext.Msg.alert('提醒', '已有調撥申請項目，不可修改!');
                    }
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    // if (T2Store.data.length == 0) {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0029/DeleteM';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                    //} else {
                    //    Ext.Msg.alert('提醒', '已有調撥申請項目，不可刪除!');
                    //}
                }
            },
            {
                itemId: 'apply', text: '提出申請', disabled: true, handler: function () {
                    if (T2Store.data.length == 0) {
                        Ext.Msg.alert('提醒', '尚未有調撥申請項目!');
                    } else {
                        Ext.MessageBox.confirm('提出申請', '是否確定提出申請？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '/api/AB0029/UpdateFLOWID0202';
                                T1Form.getForm().findField('x').setValue('A');
                                T1Form.getForm().findField('FLOWID').setValue('0202');
                                T1Submit();
                            }
                        }
                        );
                    }
                }
            }, {
                itemId: 'cancel_apply', text: '取消送申請', disabled: true, handler: function () {
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
                        Ext.MessageBox.confirm('取消送申請', '是否確定取消送申請？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0029/Cancel_apply',
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
                                            msglabel('訊息區:取消送申請成功');
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
            },{
                itemId: 'change', text: '執行調撥', disabled: true, hidden: true, handler: function () {
                    Ext.MessageBox.confirm('執行調撥', '是否確定執行調撥？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0029/UpdateFLOWID';
                            T1Form.getForm().findField('x').setValue('B');
                            T1Form.getForm().findField('FLOWID').setValue('0203');
                            T1Submit();
                        }
                    });
                }
            }, {
                itemId: 'cancel_change', text: '取消調撥', disabled: true, hidden: true, handler: function () {
                    Ext.MessageBox.confirm('取消調撥', '是否確定取消調撥？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0029/UpdateFLOWID0204';
                            T1Form.getForm().findField('x').setValue('E');
                            T1Form.getForm().findField('FLOWID').setValue('0204');
                            T1Submit();
                        }
                    });
                }
            }, {
                itemId: 'cancel_back', text: '退回', disabled: true, hidden: true, handler: function () {
                    Ext.MessageBox.confirm('退回', '是否確定退回？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0029/UpdateFLOWID0201';
                            T1Form.getForm().findField('x').setValue('F');
                            T1Form.getForm().findField('FLOWID').setValue('0201');
                            T1Submit();
                        }
                    });
                }
            }, {
                itemId: 'ok', text: '確認入庫', disabled: true, hidden: true, handler: function () {
                    T1Set = '/api/AB0029/UpdateFLOWID';
                    T1Form.getForm().findField('x').setValue('C');
                    T1Form.getForm().findField('FLOWID').setValue('0299');
                    var vDocno = T1Form.getForm().findField('DOCNO').getValue() + ',';
                    // 若有批號效期資料則需維護各批號效期撥發量
                    Ext.Ajax.request({
                        url: '/api/AA0147/ChkExp',
                        method: reqVal_p,
                        params: {
                            DOCNO: vDocno
                        },
                        //async: true,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == 'Y') {
                                    popExpForm_14(viewport, '/api/AA0147/GetExpList', {
                                        DOCNO: vDocno,
                                        CHKCOLTYPE: 1
                                    }, '確認入庫', setSubmit);
                                }
                                else {
                                    Ext.MessageBox.confirm('確認入庫', '是否確定確認入庫？', function (btn, text) {
                                        if (btn === 'yes') {
                                            setSubmit({
                                                DOCNO: vDocno,
                                                DATA_LIST: []
                                            });
                                        }
                                    });
                                }
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }, {
                itemId: 'exp', text: '效期', disabled: true, hidden: true, handler: function () {
                    var transfer = "";
                    if (fid == "0202")
                        transfer = "O"
                    else
                        transfer = "I"
                    showExpWindow(T1Form.getForm().findField('DOCNO').getValue(), transfer, viewport);
                }
            }, {
                itemId: 'export', text: '匯出', id: 'export', name: 'export', hidden: true,
                handler: function () {
                    
                    var p = new Array();
                    var p0 = T1Query.getForm().findField('P0').getValue();
                    var p1 = T1Query.getForm().findField('D1').rawValue;
                    var p2 = T1Query.getForm().findField('D2').rawValue;
                    var p3 = T1Query.getForm().findField('P3').getValue();
                    var p4 = T1Query.getForm().findField('P4').getValue();
                    var p5 = T1Query.getForm().findField('P5').getValue();
                    var p6 = T1Query.getForm().findField('P6').getValue();
                    var p7 = T1Query.getForm().findField('P7').getValue();
                    p.push({ name: 'p0', value: p0 });
                    p.push({ name: 'p1', value: p1 });
                    p.push({ name: 'p2', value: p2 });
                    p.push({ name: 'p3', value: p3 });
                    p.push({ name: 'p4', value: p4 });
                    p.push({ name: 'p5', value: p5 });
                    p.push({ name: 'p6', value: p6 });
                    p.push({ name: 'p7', value: p7 });

                    PostForm('../../../api/AB0029/Excel', p);
                    msglabel('訊息區:匯出完成');
                }
            }]
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f = T1Form.getForm();
        action = 'new';

        if (x === "I") {
            isNew = true;
            frwh_store.load({ callback: function () { if (frwh_store.getCount() > 1) f.findField('FRWH').setValue(frwh_store.getAt(0)); } });
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            f.findField('DOCNO_D').setValue('系統自編');
            u = f.findField("TOWH");
            if (towh_store.getCount() > 0) {
                f.findField('TOWH').setValue(towh_store.getAt(0).get('VALUE'));
            }
            //f.findField('TOWH').setRawValue("");
            //f.findField('TOWH').setValue("");
            T12Query.reset();
            T12Store.removeAll();
            f.findField('TOWH').setReadOnly(false);
        }
        else {
            u = f.findField('FRWH');
            frwh_store.load({ callback: function () { f.findField('FRWH').setValue(u.getValue()); } });
        }

        f.findField('x').setValue(x);
        f.findField('FRWH').setReadOnly(false);
        if (flowid == "0201")
            T1Form.down('#whBtn').setVisible(true);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
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
            items: [T1Tool]
        }
        ],

        //                // grid columns
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 100
        }, {
            text: "申請單狀態",
            dataIndex: 'FLOWID_N',
            width: 120
        }, {
            text: "申請人員",
            dataIndex: 'APPID',
            width: 100
        }, {
            text: "申請部門",
            dataIndex: 'APPDEPT',
            width: 150
            //flex: 1,
            //sortable: false
        }, {
            //  xtype: 'datecolumn',
            text: "申請日期",
            dataIndex: 'APPTIME',
            //  format: 'Xmd',
            width: 70
        }, {
            text: "調出庫房",
            dataIndex: 'FRWH_N',
            width: 150
        }, {
            text: "調入庫房",
            dataIndex: 'TOWH_N',
            width: 150
        }, {
            text: "調撥類別",
            dataIndex: 'APPLY_KIND',
            width: 120
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                        TATabs.setActiveTab('Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T1Grid.down('#cancel_apply').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('DOCNO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            DOCNO = f.findField('DOCNO').getValue();
            f.findField('DOCNO_D').setValue(DOCNO);
            DOC_FLOWID = f.findField('FLOWID').getValue();
            WEXP_ID = f.findField('LIST_ID').getValue();
            frwh_store.load();
            f.findField('TOWH').setReadOnly(true);
            f.findField('FRWH').setReadOnly(true);
            switch (fid) {
                case "0201": // AB0029
                    if (DOC_FLOWID == "0201") {
                        T1Grid.down('#edit').setDisabled(false);
                        T1Grid.down('#delete').setDisabled(false);
                        T1Grid.down('#apply').setDisabled(false);
                        T1Grid.down('#cancel_apply').setDisabled(true);
                        T2Grid.down('#add').setDisabled(false);
                    } else if (DOC_FLOWID == "0202") {
                        T1Grid.down('#edit').setDisabled(true);
                        T1Grid.down('#delete').setDisabled(true);
                        T1Grid.down('#apply').setDisabled(true);
                        T1Grid.down('#cancel_apply').setDisabled(false);
                        T2Grid.down('#add').setDisabled(true);
                    } else {
                        T1Grid.down('#edit').setDisabled(true);
                        T1Grid.down('#delete').setDisabled(true);
                        T1Grid.down('#apply').setDisabled(true);
                        T1Grid.down('#cancel_apply').setDisabled(true);
                        T2Grid.down('#add').setDisabled(true);
                    }
                    break;
                case "0202": // AB0030
                    if (DOC_FLOWID == "0202") {
                        T1Grid.down('#change').setDisabled(false);
                        T1Grid.down('#cancel_back').setDisabled(false);
                        T1Grid.down('#cancel_change').setDisabled(true);
                    }
                    else if (DOC_FLOWID == "0203") {
                        T1Grid.down('#change').setDisabled(true);
                        T1Grid.down('#cancel_back').setDisabled(true);
                        T1Grid.down('#cancel_change').setDisabled(false);
                    }
                    else {
                        T1Grid.down('#change').setDisabled(true);
                        T1Grid.down('#cancel_back').setDisabled(true);
                        T1Grid.down('#cancel_change').setDisabled(true);
                    }
                    if (WEXP_ID == "Y") {
                        if (DOC_FLOWID == "0201")
                            T1Grid.down('#exp').setDisabled(true);
                        else
                            T1Grid.down('#exp').setDisabled(false);
                    }
                    else T1Grid.down('#exp').setDisabled(true);
                    break;
                case "0203": // AB0031
                    if (DOC_FLOWID == "0203")
                        T1Grid.down('#ok').setDisabled(false);
                    else
                        T1Grid.down('#ok').setDisabled(true);
                    if (WEXP_ID == "Y") {
                        if (DOC_FLOWID == "0299" | DOC_FLOWID == "0203")
                            T1Grid.down('#exp').setDisabled(false);
                        else
                            T1Grid.down('#exp').setDisabled(true);
                    }
                    else T1Grid.down('#exp').setDisabled(true);
                    break;
            }

        }
        else {
            T1Form.getForm().reset();
            DOCNO = '';
        }
        T2Load();
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    //var T2Store = Ext.create('WEBAPP.store.ME_DOCD', {
    //    listeners: {
    //        beforeload: function (store, options) {
    //            store.removeAll();
    //            var np = {
    //                p0: DOCNO
    //            };
    //            Ext.apply(store.proxy.extraParams, np);
    //        }
    //    }
    //});
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0029/AllD',
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
                    p0: DOCNO
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        try {
            //T2Store.load({
            //    params: {
            //        start: 0
            //    }
            //});
            T2Tool.moveFirst();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }
    //T1Load();

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            T2FormMMCode.doQuery();
            var func = function () {
                var record = T2FormMMCode.store.getAt(0);
                T2FormMMCode.select(record);
                T2FormMMCode.fireEvent('select', this, record);
                T2FormMMCode.store.un('load', func);
            };
            T2FormMMCode.store.on('load', func);
        }
    }
    var T2FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0029/GetMmcodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'INV_QTY'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('FRWH').getValue()//FRWH
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('INV_QTY').setValue(r.get('INV_QTY'));
            }
        }
    });

    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        cls: 'T2b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'SEQ',
            xtype: 'hidden'
        },
        {
            xtype: 'container',
            layout: 'hbox',
            padding: '0 7 7 0',
            items: [
                T2FormMMCode,
                {
                    xtype: 'button',
                    itemId: 'btnMmcode',
                    iconCls: 'TRASearch',
                    handler: function () {
                        var f = T2Form.getForm();
                        if (!f.findField("MMCODE").readOnly) {
                            popMmcodeForm_14(viewport, '/api/AB0029/GetMmcode', {
                                WH_NO: T1Form.getForm().findField('FRWH').getValue(),
                                MMCODE: f.findField("MMCODE").getValue()
                            }, setMmcode);
                        }
                    }
                }
            ]
        },
        {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        }, {
            xtype: 'displayfield',
            fieldLabel: '計量單位',
            name: 'BASE_UNIT'
        }, {
            fieldLabel: '申請數量',
            name: 'APPQTY',
            allowBlank: false,
            maxLength: 12,
            enforceMaxLength: true,
            maskRe: /[0-9.]/,
            regexText: '最多八位正整數',
            regex: /^([0-9]{0,8}?)$/,
            fieldCls: 'required',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '現有存量',
            name: 'INV_QTY'
        }, {
            fieldLabel: '調出數量',
            name: 'APVQTY',
            maxLength: 12,
            enforceMaxLength: true,
            maskRe: /[0-9.]/,
            regexText: '最多八位正整數',
            regex: /^([0-9]{0,8}?)$/,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '應點收量',
            name: 'ACKQTY',
            maxLength: 12,
            enforceMaxLength: true,
            maskRe: /[0-9.]/,
            regexText: '最多八位正整數',
            regex: /^([0-9]{0,8}?)$/,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '調撥短少數量',
            name: 'TRNAB_QTY',
            maxLength: 12,
            enforceMaxLength: true,
            maskRe: /[0-9.]/,
            regexText: '最多八位正整數',
            regex: /^([0-9]{0,8}?)$/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (field, eOpts) {
                    if (field.getValue() == '' || field.getValue() == '0') {
                        T2Form.getForm().findField('TRNAB_RESON').setValue('');
                        T2Form.getForm().findField('TRNAB_RESON').setDisabled(true);
                    }
                    else
                        T2Form.getForm().findField('TRNAB_RESON').setDisabled(false);
                }
            }
        }, {
            xtype: 'combo',
            fieldLabel: '調撥異常原因',
            name: 'TRNAB_RESON',
            store: trnab_reson_store,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            fieldCls: 'required',
            readOnly: true
        }],
        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                var isOK = true;
                switch (fid) {
                    case "0201":
                        if (isNaN(Number(this.up('form').getForm().findField('APPQTY').getValue()))
                            || Number(this.up('form').getForm().findField('APPQTY').getValue()) <= 0) {
                            Ext.Msg.alert('提醒', '申請數量不可為0!');
                            this.up('form').getForm().findField('APPQTY').focus();
                            isOK = false;
                        }
                        if (isNaN(Number(this.up('form').getForm().findField('APPQTY').getValue()))
                            || Number.isInteger(Number(this.up('form').getForm().findField('APPQTY').getValue())) == false) {
                            Ext.Msg.alert('提醒', '申請數量需為整數');
                            this.up('form').getForm().findField('APPQTY').focus();
                            isOK = false;
                        }
                        break;
                    case "0203":
                        var ack = parseFloat(this.up('form').getForm().findField('ACKQTY').getValue());
                        var apv = parseFloat(this.up('form').getForm().findField('APVQTY').getValue());
                        var trnab = parseFloat(this.up('form').getForm().findField('TRNAB_QTY').getValue());

                        //if (isNaN(Number(this.up('form').getForm().findField('APVQTY').getValue()))
                        //    || Number.isInteger(this.up('form').getForm().findField('APVQTY').getValue()) == false) {
                        //    Ext.Msg.alert('提醒', '應點收量需為整數');
                        //    this.up('form').getForm().findField('APVQTY').focus();
                        //    isOK = false;
                        //}
                        //else
                        if (ack > apv) {
                            Ext.Msg.alert('提醒', '應點收量不可大於調出數量!');
                            this.up('form').getForm().findField('ACKQTY').focus();
                            isOK = false;
                        }
                        else if (trnab > 0) {
                            if (trnab > apv) {
                                Ext.Msg.alert('提醒', '調撥短少數量不可大於調出數量!');
                                isOK = false;
                            }
                            else {
                                var vTrnab_reson = this.up('form').getForm().findField('TRNAB_RESON').getValue();
                                if (vTrnab_reson == null || vTrnab_reson == '') {
                                    Ext.Msg.alert('提醒', '調撥短少數量大於0,調撥異常原因為必填!');
                                    isOK = false;
                                }
                            }
                        }
                }
                if (isOK) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    });
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: T2Cleanup
        }]
    });
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T2Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T2Cleanup();
                    //TATabs.setActiveTab('Query');
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
                    }
                }
            });
        }
    }
    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.findField('MMCODE').setReadOnly(true);
        f.findField('APPQTY').setReadOnly(true);
        f.findField('APVQTY').setReadOnly(true);
        f.findField('ACKQTY').setReadOnly(true);
        f.findField('TRNAB_QTY').setReadOnly(true);
        f.findField('TRNAB_RESON').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    T11Set = '../../../api/AB0029/CreateD';
                    setFormT2('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    switch (fid) {
                        case "0201":
                            T11Set = '../../../api/AB0029/UpdateD';
                            break;
                        case "0202":
                            T11Set = '../../../api/AB0029/UpdateDApvQty';
                            break;
                        case "0203":
                            T11Set = '../../../api/AB0029/UpdateDAckQty';
                            break;
                    }

                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T11Set = '../../../api/AB0029/DeleteD';
                            T2Form.getForm().findField('x').setValue('D');
                            T2Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T11Name);
        viewport.down('#form').expand();
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f2.reset();
            var r = Ext.create('T2Model');
            //var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(DOCNO);
            u = f2.findField("MMCODE");
            u.setReadOnly(false);
        }
        else {
            switch (fid) {
                case "0201":
                    u = f2.findField('APPQTY');
                    break;
                case "0202":
                    u = f2.findField('APVQTY');
                    break;
                case "0203":
                    u = f2.findField('ACKQTY');
                    break;
            }
        }
        f2.findField('x').setValue(x);
        switch (fid) {
            case "0201":
                f2.findField('APPQTY').setReadOnly(false);
                // mmcode_store.load();
                break;
            case "0202":
                f2.findField('APVQTY').setReadOnly(false);
                break;
            case "0203":
                //f2.findField('ACKQTY').setReadOnly(false);
                f2.findField('TRNAB_QTY').setReadOnly(false);
                f2.findField('TRNAB_RESON').setReadOnly(false);
                if (f2.findField('TRNAB_QTY').getValue() == '0')
                    f2.findField('TRNAB_RESON').setDisabled(true);
                else
                    f2.findField('TRNAB_RESON').setDisabled(false);
                break;
        }
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        //autoScroll: true,
        cls: 'T2',
        //defaults: {
        //    layout: 'fit'
        //},
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            flex: 0.5,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            flex: 0.5,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            align: 'right',
            format: '0,000',
            width: 80
        }, {
            text: "調出數量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            align: 'right',
            format: '0,000',
            width: 80
        }, {
            //   xtype: 'datecolumn',
            text: "調出日期",
            dataIndex: 'APVTIME',
            //   format: 'Xmd',
            width: 70
        }, {
            text: "調入數量",
            dataIndex: 'ACKQTY',
            style: 'text-align:left',
            format: '0,000',
            align: 'right',
            width: 70
        }, {
            // xtype: 'datecolumn',
            text: "調入日期",
            dataIndex: 'ACKTIME',
            // format: 'Xmd',
            width: 70
        }, {
            text: "調撥短少數量",
            dataIndex: 'TRNAB_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 100,
            visibleType: '0203'
        }, {
            text: "調撥異常原因",
            dataIndex: 'TRNAB_RESON_TEXT',
            width: 100,
            visibleType: '0203'
        }, {
            text: "實際點收量",
            dataIndex: 'GETQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100,
            visibleType: '0203',
            renderer: function (val, meta, record) {
                var pAckqty = parseFloat(record.data['ACKQTY']);
                var pTrnabqty = parseFloat(record.data['TRNAB_QTY']);
                var pGetqty = pAckqty - pTrnabqty; // 實際點收量 = 應點收量-調撥短少數量
                return pGetqty;
            }
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    //if (T2Form.hidden === true) {
                    //    T1Form.setVisible(false); T2Form.setVisible(true);
                    //}
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                        TATabs.setActiveTab('Form');
                    }
                }
            },

            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT2a() {
        T2Grid.down('#edit').setDisabled(T2Rec === 0);
        T2Grid.down('#delete').setDisabled(T2Rec === 0);

        if (T2LastRec) {
            viewport.down('#form').expand();
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('MMCODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            switch (fid) {
                case "0201":
                    if (DOC_FLOWID == "0201") {
                        //  T2Grid.down('#add').setDisabled(false);
                        T2Grid.down('#edit').setDisabled(false);
                        T2Grid.down('#delete').setDisabled(false);
                    } else {
                        //    T2Grid.down('#add').setDisabled(true);
                        T2Grid.down('#edit').setDisabled(true);
                        T2Grid.down('#delete').setDisabled(true);
                    }
                    break;
                case "0202":
                    if (DOC_FLOWID == "0202") T2Grid.down('#edit').setDisabled(false);
                    else T2Grid.down('#edit').setDisabled(true);
                    break;
                case "0203":
                    if (DOC_FLOWID == "0203") T2Grid.down('#edit').setDisabled(false);
                    else T2Grid.down('#edit').setDisabled(true);
                    break;
            }
        }
        else {
            T2Form.getForm().reset();
            viewport.down('#form').collapse();
        }
    }

    function showWin1() {
        if (win1.hidden) {
            win1.show();
        }
    }
    function hideWin1() {
        if (!win1.hidden) {
            win1.hide();
        }
    }
    if (!win1) {
        win1 = Ext.widget('window', {
            title: '',
            closeAction: 'hide',
            width: '80%',
            height: '80%',
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [
                T12Grid
            ],
            buttons: [{
                text: '選取',
                handler: function () {
                    var selection = T12Grid.getSelection();
                    if (selection.length) {
                        var idx = frwh_store.find('VALUE', selection[0].data.WH_NO);
                        if (idx >= 0)
                            T1Form.getForm().findField('FRWH').setValue(frwh_store.getAt(idx));
                        hideWin1();
                    } else Ext.Msg.alert('訊息', '請選取一筆院內碼');
                }
            }, {
                text: '關閉',
                handler: function () {
                    hideWin1();
                }
            }]
        });
    }

    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1Query]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form, T2Form]
        }]
    });
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
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        //items: [TATabs]
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });

    function Controll() {
        switch (fid) {
            case "0201":
                T2Form.getForm().findField('APVQTY').hide();
                T2Form.getForm().findField('ACKQTY').hide();
                T2Form.getForm().findField('TRNAB_QTY').hide();
                T2Form.getForm().findField('TRNAB_RESON').hide();
                //Ext.getCmp('AB0029Label').show();
                break;
            case "0202":
                T1Form.down('#whBtn').setVisible(false);
                T2Form.getForm().findField('ACKQTY').hide();
                T2Form.getForm().findField('TRNAB_QTY').hide();
                T2Form.getForm().findField('TRNAB_RESON').hide();
                T1Tool.down('#add').setVisible(false);
                T1Tool.down('#edit').setVisible(false);
                T1Tool.down('#apply').setVisible(false);
                T1Tool.down('#delete').setVisible(false);
                T1Tool.down('#change').setVisible(true);
                T1Tool.down('#cancel_change').setVisible(true);
                T1Tool.down('#cancel_back').setVisible(true);
                T1Tool.down('#exp').setVisible(true);
                T2Tool.down('#add').setVisible(false);
                T2Tool.down('#delete').setVisible(false);
                T2Form.getForm().findField('APVQTY').allowBlank = false;
                T1Grid.down('#cancel_apply').setVisible(false);
                //Ext.getCmp('AB0029Label').hide();
                break;
            case "0203":
                T1Form.down('#whBtn').setVisible(false);
                T1Tool.down('#ok').setVisible(false);
                T1Tool.down('#add').setVisible(false);
                T1Tool.down('#edit').setVisible(false);
                T1Tool.down('#apply').setVisible(false);
                T1Tool.down('#delete').setVisible(false);
                T1Tool.down('#change').setVisible(false);
                T1Tool.down('#cancel_change').setVisible(false);
                T1Tool.down('#cancel_back').setVisible(false);
                T1Tool.down('#ok').setVisible(true);
                T1Tool.down('#exp').setVisible(true);
                T2Tool.down('#add').setVisible(false);
                T2Tool.down('#delete').setVisible(false);
                T2Form.getForm().findField('ACKQTY').allowBlank = false;
                T1Grid.down('#cancel_apply').setVisible(false);
                //Ext.getCmp('AB0029Label').hide();
                break;
        }

        // 顯示/隱藏指定的欄位
        T2Grid.suspendLayouts();
        for (var i = 1; i < T2Grid.columns.length; i++) {
            if (T2Grid.columns[i].visibleType == fid || T2Grid.columns[i].visibleType == null)
                T2Grid.columns[i].setVisible(true);
            else
                T2Grid.columns[i].setVisible(false);
            if (T2Grid.columns[i].text == "調入數量" && fid == "0203")
                T2Grid.columns[i].setText("應點收量");
        }
        T2Grid.resumeLayouts(true);
    }
    Controll();
    T1Query.getForm().findField('P0').focus();
});
