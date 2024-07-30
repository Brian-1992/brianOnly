Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Get = '../../../api/CD0003/GetAll';
    var T1Set = '../../../api/CD0003/Delete';
    var T2Get = '../../../api/CD0003/GetID';
    var viewModel = Ext.create('WEBAPP.store.CD0003VM');
    var T1Name = '庫房揀貨員資料';
    var T2Name = '揀貨員資料';
    var T1Result = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // ***** T1 庫房選揀貨人員 *****
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'WH_NAME', 'WH_USERID', 'IS_DUTY', 'WH_KIND', 'WH_GRADE']
    });

    var T1Store = viewModel.getStore('BC_WHID', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    //p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        //T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('WH_NO').getValue());
        T1Store.getProxy().setExtraParam("p0", T1Result);
        //if (T2Store.getCount() > 0) {
        //    T2Load();
        //}
        T1Store.load({
            params: {
                start: 0
            }
        });
    }
    //T1Load();

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'WH_NO',
        fieldLabel: '庫房代碼',
        fieldCls: 'required',
        allowBlank: false,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/CD0003/GetWH_NoCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            //var f = T2Form.getForm();
            //if (!f.findField("MMCODE").readOnly) {
            //    tmpArray = f.findField("FRWH2").getValue().split(' ');
            //    return {
            //        //MMCODE: f.findField("MMCODE").getValue(),
            //        WH_NO: tmpArray[0]
            //    };
            //}
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                //var f = T2Form.getForm();
                //if (r.get('MMCODE') !== '') {
                //    f.findField("MMCODE").setValue(r.get('MMCODE'));
                //    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                //    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                //    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                //}
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 60
        },

        items: [{
            xtype: 'container',
            layout: 'hbox',
            //padding: '0 7 7 0',
            items: [
                wh_NoCombo,
                {
                    xtype: 'button',
                    itemId: 'btnWh_no',
                    iconCls: 'TRASearch',
                    handler: function () {
                        var f = T1Query.getForm();
                        //if (!f.findField("MMCODE").readOnly) {
                        //    tmpArray = f.findField("FRWH2").getValue().split(' ');
                        popWh_noForm(viewport, '/api/CD0003/GetWh_no', { WH_NO: f.findField("WH_NO").getValue() }, setWh_no);
                        //}
                    }
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (!T1Query.getForm().findField('WH_NO').getValue())
                            Ext.MessageBox.alert('錯誤', '尚未選取庫房代碼');
                        else {
                            Ext.Ajax.request({
                                url: '/api/CD0003/CheckWhExist',
                                method: reqVal_p,
                                params: {
                                    p0: T1Query.getForm().findField('WH_NO').getValue()
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    T1Store.removeAll();
                                    if (data.success) {
                                        T1Result = T1Query.getForm().findField('WH_NO').getValue();
                                        T1Load();
                                        T2Grid.down('#btnQueryD').setDisabled(false);
                                    }
                                    else {
                                        Ext.MessageBox.alert('錯誤', '此庫房代碼不存在');
                                        T1Result = '';
                                        T2Store.removeAll();
                                        T2Grid.down('#btnQueryD').setDisabled(true);
                                    }
                                },
                                failure: function (response, options) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    T2Grid.down('#btnQueryD').setDisabled(true);
                                }
                            });

                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                    }
                }
            ]
        }]
    });


    function setWh_no(args) {
        if (args.WH_NO !== '') {
            T1Query.getForm().findField("WH_NO").setValue(args.WH_NO);
        }
    }


    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        items: [
            {
                itemId: 'delete',
                name: 'delete',
                text: '刪除',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    let wh_no = '';
                    let wh_userid = '';
                    //selection.map(item => {
                    //    wh_no += item.get('WH_NO') + ',';
                    //    wh_userid += item.get('WH_USERID') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        wh_no += item.get('WH_NO') + ',';
                        wh_userid += item.get('WH_USERID') + ',';
                    })
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/CD0003/Delete',
                                method: reqVal_p,
                                params: {
                                    WH_NO: wh_no,
                                    WH_USERID: wh_userid
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('資料刪除成功');
                                        T1Load();
                                        if (!T2Query.getForm().findField('P0').getValue() && !T2Query.getForm().findField('P1').getValue() && !T2Query.getForm().findField('P2').getValue()) {
                                            T2Store.removeAll();
                                        }
                                        else {
                                            T2Load();
                                        }
                                        //Ext.getCmp('btnSubmit').setDisabled(true);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            })
                        }
                    })
                },
            }
        ],
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }
        ],

        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',

        // grid columns
        columns: [{
            xtype: 'rownumberer',
            width: 50
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80,
            sortable: true
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 160,
            sortable: true
        }, {
            text: "揀貨人員代碼",
            dataIndex: 'WH_USERID',
            width: 80,
            sortable: true
        }, {
            text: "揀貨人員姓名",
            dataIndex: 'UNA',
            width: 120,
            sortable: true
        }, {
            text: "是否值日生",
            dataIndex: 'IS_DUTY',
            width: 80,
            sortable: true
        }, {
            xtype: 'hidden',
            text: "庫房分類",
            dataIndex: 'WH_KIND_D',
            width: 80,
            sortable: true
        }, {
            xtype: 'hidden',
            text: "庫房級別",
            dataIndex: 'WH_GRADE_D',
            width: 80,
            sortable: true
        }, {
            header: "",
            flex: 1
        }],

        listeners: {
            selectionchange: function (model, records) {
                T1Grid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    T1Form.loadRecord(records[0]);
                }
            }
        }
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        items: [{
            name: 'x'
        }, {
            name: 'WH_NO'
        }, {
            name: 'WH_NAME'
        }, {
            name: 'WH_USERID'
        }, {
            name: 'IS_DUTY'
        }, {
            name: 'WH_KIND'
        }, {
            name: 'WH_GRADE'
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
                    var r = form.getRecord();
                    switch (form.findField("x").getValue()) {
                        case "I":
                            var v = form.getValues();//getFieldValues(); 
                            //r.set(v);
                            //修正重覆問題2013/09/09 思評
                            var r = Ext.create('T1Model');
                            r.set(v);

                            T1Store.insert(0, r);
                            r.commit();
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('資料刪除成功');
                            break;
                    }
                    T2Store.load();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('錯誤', MMIS.Message.clientError);
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('錯誤', MMIS.Message.communicationError);
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('錯誤', action.result.msg);
                    }
                }
            });
        }
    }


    // ***** T2 揀貨人員資料 *****    
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['TUSER', 'UNA', 'INID', 'INID_NAME']
    });

    var T2Store = viewModel.getStore('UR_ID', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T2Query.getForm().findField('P0').getValue(),
                    p1: T2Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            //load: function (store, records, successful, eOpts) {
            //T1Store.removeAll();
            //}
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T2Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T2Load() {
        T2Store.getProxy().setExtraParam("p0", T2Query.getForm().findField('P0').getValue());
        T2Store.getProxy().setExtraParam("p1", T2Query.getForm().findField('P1').getValue());
        T2Store.getProxy().setExtraParam("p2", T2Query.getForm().findField('P2').getValue());
        T2Store.getProxy().setExtraParam("p3", T1Result);
        T2Tool.moveFirst();
    }
    //T2Load();

    var InidCombo = Ext.create('WEBAPP.form.InidWhCombo', {
        name: 'P2',
        fieldLabel: '責任中心代碼',
        //fieldCls: 'required',
        allowBlank: true,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/CD0003/GetInidWhCombo',

        //查詢完會回傳的欄位
        //extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            //var f = T2Form.getForm();
            //if (!f.findField("MMCODE").readOnly) {
            //    tmpArray = f.findField("FRWH2").getValue().split(' ');
            //    return {
            //        //MMCODE: f.findField("MMCODE").getValue(),
            //        WH_NO: tmpArray[0]
            //    };
            //}
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                //var f = T2Form.getForm();
                //if (r.get('MMCODE') !== '') {
                //    f.findField("MMCODE").setValue(r.get('MMCODE'));
                //    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                //    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                //    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                //}
            }
        }
    });


    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 80
        },

        items: [{
            fieldLabel: '人員代碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 60,
            width: 200,
            labelWidth: 60,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '中文姓名',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 60,
            width: 200,
            padding: '0 4 0 4'
        }, InidCombo,
        {
            xtype: 'button',
            text: '查詢',
            id: 'btnQueryD',
            disabled: true,
            handler: function () {
                if (!T2Query.getForm().findField('P0').getValue() && !T2Query.getForm().findField('P1').getValue() && !T2Query.getForm().findField('P2').getValue()) {
                    Ext.MessageBox.alert('錯誤', '請至少輸入一項查詢條件資料');
                    return;
                }
                else {
                    T2Load();
                }
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }]
    });


    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'insert',
                name: 'insert',
                text: '指定',
                disabled: true,
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (!T1Result) {
                        Ext.MessageBox.alert('錯誤', '尚未選取庫房代碼');
                        return;
                    }
                    else if (selection.length) {
                        let tuser = '';
                        let name = '';
                        let wh_no = T1Result;
                        //selection.map(item => {
                        //    name += '「' + item.get('TUSER') + '」<br>';
                        //    tuser += item.get('TUSER') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('TUSER') + '」<br>';
                            tuser += item.get('TUSER') + ',';
                        })

                        Ext.MessageBox.confirm('指定', '是否確定將下列人員加入庫房 ' + wh_no + ' 揀貨作業?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/CD0003/Insert',
                                    method: reqVal_p,
                                    params: {
                                        WH_NO: T1Result,
                                        TUSER: tuser
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('儲存庫房揀貨人員資料成功');
                                            //Ext.MessageBox.alert('訊息', '指定院內碼<br>' + mmcode + '成功');
                                            //T2Store.removeAll();
                                            //T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            T2Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                        else {
                                            Ext.MessageBox.alert('錯誤', '此庫房已包含此人員資料，將重新顯示資料');
                                            T1Load();
                                            T2Load();
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    },
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取');
                        return;
                    }
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: T2Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T2Tool]
        }
        ],

        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',

        // grid columns
        columns: [{
            xtype: 'rownumberer',
            width: 50
        }, {
            text: "人員代碼",
            dataIndex: 'TUSER',
            width: 80,
            sortable: true
        }, {
            text: "中文姓名",
            dataIndex: 'UNA',
            width: 120,
            sortable: true
        }, {
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 80,
            sortable: true
        }, {
            text: "責任中心名稱",
            dataIndex: 'INID_NAME',
            width: 300,
            sortable: true
        }, {
            header: "",
            flex: 1
        }],

        listeners: {
            selectionchange: function (model, records) {
                if (T1Result != '' && records.length > 0) {
                    T2Grid.down('#insert').setDisabled(false);
                }
                else {
                    T2Grid.down('#insert').setDisabled(true);
                }

                if (records[0]) {
                    T2Form.loadRecord(records[0]);
                }
            }
        }
    });

    var T2Form = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        items: [{
            name: 'x'
        }, {
            name: 'TUSER'
        }, {
            name: 'UNA'
        }, {
            name: 'INID'
        }, {
            name: 'INID_NAME'
        }]
    });

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
            split: true,
            collapsible: false,
            border: false,
            height: '50%',
            items: [T1Grid]
        }, {
            itemId: 't2Grid',
            region: 'south',
            layout: 'fit',
            split: true,
            collapsible: true,
            border: false,
            height: '50%',
            items: [T2Grid]
        }]
    });
});





