Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    var T11Get = '../../../api/AA0059/AllMI_WHMM';
    var T11Set = '../../../api/AA0059/Delete';
    var T12Get = '../../../api/AA0059/AllMI_WHMAST';
    var T12Set = '';
    var T21Get = '../../../api/AA0059/AllMI_MMWH';
    var T21Set = '../../../api/AA0059/Delete';
    var T22Get = '../../../api/AA0059/AllMI_MAST';
    var T22Set = '';
    var viewModel = Ext.create('WEBAPP.store.AA.AA0059VM');
    var T11Name = '庫房院內碼資料';
    var T12Name = '院內碼資料';
    var T21Name = '院內碼庫房資料';
    var T22Name = '庫房資料';
    var T11Result = '';
    var T21Result = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    // ***** T11 庫房選院內碼 *****
    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'WH_NAME', 'MMCODE', 'MMNAME_E', 'E_DRUGCLASS', 'E_DRUGCLASSIFY', 'WH_KIND', 'WH_GRADE']
    });

    var T11Store = Ext.create('Ext.data.Store', {
        model: 'T11Model',

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T11Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var T11Store = viewModel.getStore('MI_WHMM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T11Query.getForm().findField('P0').getValue(),
                    p1: T11Query.getForm().findField('P1').getValue(),
                    p4: T11Query.getForm().findField('P4').getValue(),
                    p5: T11Query.getForm().findField('P5').getValue(),
                    p6: T11Query.getForm().findField('P6').getValue(),
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
            url: T11Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T11Load() {
        //T11Store.getProxy().setExtraParam("p0", T11Query.getForm().findField('WH_NO').getValue());
        T11Store.getProxy().setExtraParam("p0", T11Query.getForm().findField('WH_NO').getValue());
        //if (T12Store.getCount() > 0) {
        //    T12Load();
        //}
        T11Store.load({
            params: {
                start: 0
            }
        });
    }
    //T11Load();

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'WH_NO',
        fieldLabel: '庫房代碼',
        fieldCls: 'required',
        allowBlank: true,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0059/GetWH_NoCombo',

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

    var T11Query = Ext.widget({
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
                    //iconCls: 'TRASearch',
                    hidden: true,
                    handler: function () {
                        var f = T11Query.getForm();
                        //if (!f.findField("MMCODE").readOnly) {
                        //    tmpArray = f.findField("FRWH2").getValue().split(' ');
                        popWh_noForm(viewport, '/api/AA0059/GetWh_no', { WH_NO: f.findField("WH_NO").getValue() }, setWh_no);
                        //}
                    }
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T11Store.removeAll();
                        T11Load();
                        T12Store.removeAll();
                        //if (!T11Query.getForm().findField('WH_NO').getValue())
                        //    Ext.MessageBox.alert('錯誤', '尚未選取庫房代碼');
                        //else {
                        //    Ext.Ajax.request({
                        //        url: '/api/AA0059/CheckWhExist',
                        //        method: reqVal_p,
                        //        params: {
                        //            p0: T11Query.getForm().findField('WH_NO').getValue()
                        //        },
                        //        success: function (response) {
                        //            var data = Ext.decode(response.responseText);
                        //            T11Store.removeAll();
                        //            if (data.success) {
                        //                T11Result = T11Query.getForm().findField('WH_NO').getValue();
                        //                T11Load();
                        //                T12Store.removeAll();
                        //            }
                        //            else {
                        //                Ext.MessageBox.alert('錯誤', '庫房代碼不存在');
                        //                T11Result = '';
                        //                T12Store.removeAll();
                        //            }
                        //        },
                        //        failure: function (response, options) {
                        //            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        //        }
                        //    });
                        //}
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                    }
                },
                {
                    xtype: 'displayfield',

                }
            ]
        }]
    });


    function setWh_no(args) {
        if (args.WH_NO !== '') {
            T11Query.getForm().findField("WH_NO").setValue(args.WH_NO);
        }
    }


    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
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
                    var selection = T11Grid.getSelection();
                    let wh_no = '';
                    let mmcode = '';
                    //selection.map(item => {
                    //    wh_no += item.get('WH_NO') + ',';
                    //    mmcode += item.get('MMCODE') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        wh_no += item.get('WH_NO') + ',';
                        mmcode += item.get('MMCODE') + ',';
                    })
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0059/Delete',
                                method: reqVal_p,
                                params: {
                                    WH_NO: wh_no,
                                    MMCODE: mmcode
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('資料刪除成功');
                                        T11Load();
                                        T12Load();
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

    var T11Grid = Ext.create('Ext.grid.Panel', {
        //title: T11Name,
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T11Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T11Tool]
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
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 380,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 380,
            sortable: true
        }
            /*   , {
               text: "用藥類別",
               dataIndex: 'E_DRUGCLASS_D',
               width: 80,
               sortable: true
           }, {
               text: "藥品性質",
               dataIndex: 'E_DRUGCLASSIFY_D',
               width: 120,
               sortable: true
           }, {
               text: "庫房分類",
               dataIndex: 'WH_KIND_D',
               width: 80,
               sortable: true
           }, {
               text: "庫房級別",
               dataIndex: 'WH_GRADE_D',
               width: 80,
               sortable: true
           }*/
            , {
            header: "",
            flex: 1
        }],

        listeners: {
            selectionchange: function (model, records) {
                T11Grid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    T11Form.loadRecord(records[0]);
                }
            }
        }
    });

    var T11Form = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        items: [{
            name: 'x'
        }, {
            name: 'WH_NO'
        }, {
            name: 'WH_NAME'
        }, {
            name: 'MMCODE'
        }, {
            name: 'MMNAME_E'
        }, {
            name: 'E_DRUGCLASS'
        }, {
            name: 'E_DRUGCLASSIFY'
        }, {
            name: 'WH_KIND'
        }, {
            name: 'WH_GRADE'
        }]
    });

    function T11Submit() {
        var f = T11Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var r = form.getRecord();
                    switch (form.findField("x").getValue()) {
                        case "I":
                            var v = form.getValues();//getFieldValues(); 
                            //r.set(v);
                            //修正重覆問題2013/09/09 思評
                            var r = Ext.create('T11Model');
                            r.set(v);

                            T11Store.insert(0, r);
                            r.commit();
                            break;
                        case "D":
                            T11Store.remove(r);
                            r.commit();
                            msglabel('資料刪除成功');
                            break;
                    }
                    T12Store.load();
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


    // ***** T12 院內碼資料 *****    
    Ext.define('T12Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_E', 'E_MANUFACT', 'E_DRUGCLASS_D', 'E_DRUGCLASSIFY_D', 'E_SOURCECODE_D', 'E_DRUGAPLTYPE_D']
    });

    var T12Store = viewModel.getStore('MI_MAST', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T12Query.getForm().findField('P0').getValue(),
                    p1: T12Query.getForm().findField('P1').getValue(),
                    p4: T12Query.getForm().findField('P4').getValue(),
                    p5: T12Query.getForm().findField('P5').getValue(),
                    p6: T12Query.getForm().findField('P6').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            //load: function (store, records, successful, eOpts) {
            //T11Store.removeAll();
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
            url: T12Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T12Load() {
        T12Store.getProxy().setExtraParam("p0", T12Query.getForm().findField('P0').getValue());
        T12Store.getProxy().setExtraParam("p1", T12Query.getForm().findField('P1').getValue());
        T12Store.getProxy().setExtraParam("p2", T11Query.getForm().findField('WH_NO').getValue());
        T12Store.getProxy().setExtraParam("p3", T11Result);
        T12Store.getProxy().setExtraParam("p4", T12Query.getForm().findField('P4').getValue());
        T12Store.getProxy().setExtraParam("p5", T12Query.getForm().findField('P5').getValue());
        T12Store.getProxy().setExtraParam("p6", T12Query.getForm().findField('P6').getValue());
        T12Store.load({
            params: {
                start: 0
            }
        });
    }
    //T12Load();

    var T12Query = Ext.widget({
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
            fieldLabel: '院內碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 10,
            width: 150,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '英文品名',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 60,
            width: 170,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '中文品名',
            name: 'P4',
            enforceMaxLength: true,
            maxLength: 60,
            width: 170,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '原製造商',
            name: 'P5',
            enforceMaxLength: true,
            maxLength: 60,
            width: 170,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '廠牌',
            name: 'P6',
            enforceMaxLength: true,
            maxLength: 60,
            width: 170,
            padding: '0 4 0 4'
        }, {
            xtype: 'button',
            text: '查詢',
            handler: T12Load
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

    var T12Tool = Ext.create('Ext.PagingToolbar', {
        store: T12Store,
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
                    var selection = T12Grid.getSelection();

                    if (!T11Query.getForm().findField('WH_NO').getValue())
                        Ext.MessageBox.alert('錯誤', '尚未選取庫房代碼');
                    else {
                        Ext.Ajax.request({
                            url: '/api/AA0059/CheckWhExist',
                            method: reqVal_p,
                            params: {
                                p0: T11Query.getForm().findField('WH_NO').getValue()
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    T11Result = T11Query.getForm().findField('WH_NO').getValue();

                                    if (selection.length) {
                                        let mmcode = '';
                                        let name = '';
                                        let wh_no = T11Result;
                                        //selection.map(item => {
                                        //    name += '「' + item.get('MMCODE') + '」<br>';
                                        //    mmcode += item.get('MMCODE') + ',';
                                        //});
                                        $.map(selection, function (item, key) {
                                            name += '「' + item.get('MMCODE') + '」<br>';
                                            mmcode += item.get('MMCODE') + ',';
                                        })

                                        Ext.MessageBox.confirm('指定', '確認是否指定庫房代碼 ' + wh_no + ' 至院內碼<br>' + name, function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/AA0059/InsertMM',
                                                    method: reqVal_p,
                                                    params: {
                                                        WH_NO: T11Result,
                                                        MMCODE: mmcode
                                                    },
                                                    //async: true,
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            msglabel('指定庫房代碼成功');
                                                            //Ext.MessageBox.alert('訊息', '指定院內碼<br>' + mmcode + '成功');
                                                            //T2Store.removeAll();
                                                            //T1Grid.getSelectionModel().deselectAll();
                                                            //T1Load();
                                                            T11Load();
                                                            T12Load();
                                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                                        }
                                                        else {
                                                            Ext.MessageBox.alert('錯誤', '此庫房已包含所選院內碼資料，將重新顯示資料');
                                                            T11Load();
                                                            T12Load();
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
                                else {
                                    Ext.MessageBox.alert('錯誤', '庫房代碼不存在');
                                    T11Result = '';
                                    T12Store.removeAll();
                                }
                            },
                            failure: function (response, options) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                }
            }
        ]
    });

    var T12Grid = Ext.create('Ext.grid.Panel', {
        store: T12Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T12Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T12Tool]
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
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 380,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 380,
            sortable: true
        }, {
            text: "原製造商",
            dataIndex: 'E_MANUFACT',
            width: 380,
            sortable: true
        }, {
            text: "廠牌",
            dataIndex: 'M_AGENLAB',
            width: 380,
            sortable: true
        }
            /*, {
            text: "用藥類別",
            dataIndex: 'E_DRUGCLASS_D',
            width: 80,
            sortable: true
        }, {
            text: "藥品性質",
            dataIndex: 'E_DRUGCLASSIFY_D',
            width: 120,
            sortable: true
        }, {
            text: "來源代碼	",
            dataIndex: 'E_SOURCECODE_D',
            width: 80,
            sortable: true
        }, {
            text: "藥品請領類別",
            dataIndex: 'E_DRUGAPLTYPE_D',
            width: 80,
            sortable: true
        }*/
            , {
            header: "",
            flex: 1
        }],

        listeners: {
            selectionchange: function (model, records) {
                //T12Grid.down('#insert').setDisabled(records.length === 0);
                if ((T11Query.getForm().findField('WH_NO').getValue() != null && T11Query.getForm().findField('WH_NO').getValue().trim() != '')
                     && records.length > 0) {
                    T12Grid.down('#insert').setDisabled(false);
                }
                else {
                    T12Grid.down('#insert').setDisabled(true);
                }
                if (records[0]) {
                    T12Form.loadRecord(records[0]);
                }
            }
        }
    });

    var T12Form = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        items: [{
            name: 'x'
        }, {
            name: 'MMCODE'
        }, {
            name: 'MMNAME_E'
        }, {
            name: 'E_MANUFACT'
        }, {
            name: 'E_DRUGCLASS_D'
        }, {
            name: 'E_DRUGCLASSIFY_D'
        }, {
            name: 'E_SOURCECODE_D'
        }, {
            name: 'E_DRUGAPLTYPE_D'
        }]
    });


    // ***** T21 院內碼選庫房 *****
    Ext.define('T21Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_E', 'WH_NO', 'WH_NAME', 'E_DRUGCLASS', 'E_DRUGCLASSIFY', 'WH_KIND', 'WH_GRADE']
    });

    var T21Store = Ext.create('Ext.data.Store', {
        model: 'T21Model',

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T21Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var T21Store = viewModel.getStore('MI_MMWH', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T21Query.getForm().findField('P0').getValue(),
                    p1: T21Query.getForm().findField('P1').getValue(),
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
            url: T21Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T21Load() {
        //T21Store.getProxy().setExtraParam("p0", T21Query.getForm().findField('MMCODE').getValue());
        T21Store.getProxy().setExtraParam("p0", T21Result);
        //if (T22Store.getCount() > 0) {
        //    T22Load();
        //}
        T21Store.load({
            params: {
                start: 0
            }
        });
    }
    //T21Load();

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0059/GetMMCodeCombo',

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

    var T21Query = Ext.widget({
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

        items: [
            //{
            //    xtype: 'textfield',
            //    fieldLabel: '院內碼',
            //    name: 'MMCODE',
            //    //editable: false,
            //    submitValue: true,
            //    readOnly: false,
            //    allowBlank: false,
            //    //fieldCls: 'required',
            //    listeners: {
            //        render: function () {
            //            this.getEl().on('mousedown', function (e, t, eOpts) {
            //                //var f = T21Query.getForm();
            //                //if (!f.findField("MMCODE").readOnly) {
            //                if (!T21Query.getForm().findField("MMCODE").readOnly) {
            //                    //tmpArray = f.findField("MAT_CLASS2").getValue().split(' ');
            //                    popMmcodeForm(viewport, { closeCallback: setMmcode });
            //                    //popMmcodeForm(viewport, { MMCODE: f.findField("MMCODE").getValue(), MAT_CLASS: '' , closeCallback: setMmcode });
            //                }
            //            });
            //        }
            //    }
            //},
            {
                xtype: 'container',
                layout: 'hbox',
                //padding: '0 7 7 0',
                items: [
                    mmCodeCombo,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        //iconCls: 'TRASearch',
                        hidden: true,
                        handler: function () {
                            var f = T21Query.getForm();
                            //if (!f.findField("MMCODE").readOnly) {
                            //    tmpArray = f.findField("FRWH2").getValue().split(' ');
                            popMmcodeForm(viewport, '/api/AA0059/GetMmcode', { MMCODE: f.findField("MMCODE").getValue() }, setMmcode);
                            //}
                        }
                    },

                ]
            },
            {
                xtype: 'button',
                text: '查詢',
                //iconCls: 'TRASearch',
                handler: function () {
                    if (!T21Query.getForm().findField('MMCODE').getValue())
                        Ext.MessageBox.alert('錯誤', '尚未選取院內碼');
                    else {
                        Ext.Ajax.request({
                            url: '/api/AA0059/CheckMmExist',
                            method: reqVal_p,
                            params: {
                                p0: T21Query.getForm().findField('MMCODE').getValue()
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                T21Store.removeAll();
                                if (data.success) {
                                    T21Result = T21Query.getForm().findField('MMCODE').getValue();
                                    T21Load();
                                    T22Store.removeAll();
                                }
                                else {
                                    Ext.MessageBox.alert('錯誤', '院內碼不存在');
                                    T21Result = '';
                                    T22Store.removeAll();
                                }
                            },
                            failure: function (response, options) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                }
            },
            {
                xtype: 'button',
                text: '清除',
                //iconCls: 'TRAClear',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus();
                }
            }]
    });

    function setMmcode(args) {
        if (args.MMCODE !== '') {
            T21Query.getForm().findField("MMCODE").setValue(args.MMCODE);
        }
    }

    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
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
                    var selection = T21Grid.getSelection();
                    let wh_no = '';
                    let mmcode = '';
                    //selection.map(item => {
                    //    wh_no += item.get('WH_NO') + ',';
                    //    mmcode += item.get('MMCODE') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        wh_no += item.get('WH_NO') + ',';
                        mmcode += item.get('MMCODE') + ',';
                    })
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0059/Delete',
                                method: reqVal_p,
                                params: {
                                    WH_NO: wh_no,
                                    MMCODE: mmcode
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('資料刪除成功');
                                        T21Load();
                                        T22Load();
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

    var T21Grid = Ext.create('Ext.grid.Panel', {
        //title: T21Name,
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T21Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T21Tool]
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
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 380,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 380,
            sortable: true
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
            text: "用藥類別",
            dataIndex: 'E_DRUGCLASS_D',
            width: 80,
            sortable: true
        }, {
            text: "藥品性質",
            dataIndex: 'E_DRUGCLASSIFY_D',
            width: 120,
            sortable: true
        }, {
            text: "庫房分類",
            dataIndex: 'WH_KIND_D',
            width: 80,
            sortable: true
        }, {
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
                T21Grid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    T21Form.loadRecord(records[0]);
                }
            }
        }
    });

    var T21Form = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        items: [{
            name: 'x'
        }, {
            name: 'MMCODE'
        }, {
            name: 'MMNAME_E'
        }, {
            name: 'WH_NO'
        }, {
            name: 'WH_NAME'
        }, {
            name: 'E_DRUGCLASS'
        }, {
            name: 'E_DRUGCLASSIFY'
        }, {
            name: 'WH_KIND'
        }, {
            name: 'WH_GRADE'
        }]
    });

    function T21Submit() {
        var f = T21Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T21Set,
                success: function (form, action) {
                    myMask.hide();
                    var r = form.getRecord();
                    switch (form.findField("x").getValue()) {
                        case "I":
                            var v = form.getValues();//getFieldValues(); 
                            //r.set(v);
                            //修正重覆問題2013/09/09 思評
                            var r = Ext.create('T21Model');
                            r.set(v);

                            T21Store.insert(0, r);
                            r.commit();
                            break;
                        case "D":
                            T21Store.remove(r);
                            r.commit();
                            msglabel('資料刪除成功');
                            break;
                    }
                    T22Store.load();
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


    // ***** T22 庫房資料 *****
    Ext.define('T22Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'WH_NAME', 'WH_KIND_D', 'WH_GRADE_D']
    });

    var T22Store = viewModel.getStore('MI_WHMAST', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T22Query.getForm().findField('P0').getValue(),
                    p1: T22Query.getForm().findField('P1').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            //load: function (store, records, successful, eOpts) {
            //T11Store.removeAll();
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
            url: T22Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T22Load() {
        T22Store.getProxy().setExtraParam("p0", T22Query.getForm().findField('P0').getValue());
        T22Store.getProxy().setExtraParam("p1", T22Query.getForm().findField('P1').getValue());
        T22Store.getProxy().setExtraParam("p2", T21Query.getForm().findField('MMCODE').getValue());
        T22Store.getProxy().setExtraParam("p3", T21Result);
        T22Store.load({
            params: {
                start: 0
            }
        });
    }
    //T22Load();

    var T22Query = Ext.widget({
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
            fieldLabel: '庫房代碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 10,
            width: 150,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '庫房名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 60,
            width: 170,
            padding: '0 4 0 4'
        }, {
            xtype: 'button',
            text: '查詢',
            //iconCls: 'TRASearch',
            handler: T22Load
        }, {
            xtype: 'button',
            text: '清除',
            //iconCls: 'TRAClear',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }]
    });

    var T22Tool = Ext.create('Ext.PagingToolbar', {
        store: T22Store,
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
                    var selection = T22Grid.getSelection();
                    if (!T21Result) {
                        Ext.MessageBox.alert('錯誤', '尚未選取院內碼');
                        return;
                    }
                    else if (selection.length) {
                        let mmcode = T21Result;
                        let name = '';
                        let wh_no = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('WH_NO') + '」<br>';
                        //    wh_no += item.get('WH_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('WH_NO') + '」<br>';
                            wh_no += item.get('WH_NO') + ',';
                        })

                        Ext.MessageBox.confirm('指定', '確認是否指定院內碼 ' + mmcode + ' 至庫房代碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0059/InsertWH',
                                    method: reqVal_p,
                                    params: {
                                        WH_NO: wh_no,
                                        MMCODE: T21Result
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('指定院內碼成功');
                                            //Ext.MessageBox.alert('訊息', '指定院內碼<br>' + mmcode + '成功');
                                            //T2Store.removeAll();
                                            //T1Grid.getSelectionModel().deselectAll();
                                            //T1Load();
                                            T21Load();
                                            T22Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                        else {
                                            Ext.MessageBox.alert('錯誤', '此院內碼已包含所選庫房資料，將重新顯示資料');
                                            T21Load();
                                            T22Load();
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

    var T22Grid = Ext.create('Ext.grid.Panel', {
        store: T22Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T22Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T22Tool]
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
            text: "庫房分類",
            dataIndex: 'WH_KIND_D',
            width: 80,
            sortable: true
        }, {
            text: "庫房級別",
            dataIndex: 'WH_GRADE_D',
            width: 80,
            sortable: true
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                //T22Grid.down('#insert').setDisabled(records.length === 0);
                if (T21Result != '' && records.length > 0) {
                    T22Grid.down('#insert').setDisabled(false);
                }
                else {
                    T22Grid.down('#insert').setDisabled(true);
                }

                if (records[0]) {
                    T22Form.loadRecord(records[0]);
                }
            }
        }
    });

    var T22Form = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        items: [{
            name: 'x'
        }, {
            name: 'WH_NO'
        }, {
            name: 'WH_NAME'
        }, {
            name: 'WH_KIND_D'
        }, {
            name: 'WH_GRADE_D'
        }]
    });



    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義TAB內容 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                switch (newCard.title) {
                    case "庫房選院內碼":
                        //Grid_Cleanup(T11Query, T11Grid, T11Tool);
                        //Ext.getCmp('').setValue();
                        //T11Load(); //自動帶入資料
                        //Grid_Cleanup(T12Query, T12Grid, T12Tool);
                        T12Grid.down('').setDisabled();
                        break;
                    case "院內碼選庫房":
                        //Grid_Cleanup(T21Query, T21Grid, T21Tool);
                        //Ext.getCmp('').setValue();
                        //Grid_Cleanup(T22Query, T22Grid, T22Tool);
                        T22Grid.down('').setDisabled();
                        break;
                }
                T11Rec = 0;
                T11LastRec = null;
            }
        },
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            title: '庫房選院內碼',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                itemId: 't11Grid',
                region: 'center',
                layout: 'fit',
                split: true,
                collapsible: false,
                border: false,
                height: '50%',
                items: [T11Grid]
            }, {
                title: T12Name,
                itemId: 't12Grid',
                region: 'south',
                layout: 'fit',
                split: true,
                collapsible: true,
                border: false,
                height: '50%',
                items: [T12Grid]
            }]
        }
            //, {
            //title: '院內碼選庫房',
            //layout: 'border',
            //padding: 0,
            //split: true,
            //items: [{
            //    itemId: 't21Grid',
            //    region: 'center',
            //    layout: 'fit',
            //    split: true,
            //    collapsible: false,
            //    border: false,
            //    height: '50%',
            //    items: [T21Grid]
            //}, {
            //    title: T22Name,
            //    itemId: 't22Grid',
            //    region: 'south',
            //    layout: 'fit',
            //    split: true,
            //    collapsible: true,
            //    border: false,
            //    height: '50%',
            //    items: [T22Grid]
            //}]
            //},
        ]
    });


    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 網頁進入點 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [TATabs]
    });

    /* T1Form.on('dirtychange', function (basic, dirty, eOpts) {
        window.onbeforeunload = dirty ? my_onbeforeunload : null;
    }); */


    //清空查詢列、Grid顯示資料、頁次顯示
    function Grid_Cleanup(Query, Grid, PagingTool) {
        Query.getForm().reset(); //清空
        Grid.store.removeAll();  //清空Grid內容
        PagingTool.onLoad();     //Bingo
    }
});
