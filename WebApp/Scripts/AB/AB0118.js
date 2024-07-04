
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.ImageGridField']);
Ext.onReady(function () {
    // var T1Get = '/api/AB0118/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "衛材一般物品庫備申請";
    var reportUrl = '/Report/A/AB0118.aspx';
    var T1Rec = 0;
    var T8Rec = 0;
    var T1LastRec = null;
    var T8LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var arrP2 = ["1", "11"];
    var T6GetExcel = '../../../api/AB0118/Excel';
    var detailExport = '../../../api/AB0118/DetailExcel';
    var getDocAppAmout = '/api/AB0118/GetDocAppAmout';
    var LastHIS14SUPDET = "123";
    var storeidColorName = false;
    var hosp_code = '';
    var setCancelHidden = false;
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');

    var st_pkdocno = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1F2,
                    p1: T1F4,
                    p2: T1F8
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetDocnopkCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        }
    });
    var st_pknote = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1F2,
                    p1: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetDocpknoteCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        }
    });
    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });


    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_reason = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetReasonCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_towhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_getlogininfo = Ext.create('Ext.data.Store', {
        listeners: {
            load: function (store, eOpts) {
                hosp_code = store.getAt(0).get('HOSP_CODE');
                // 依取得的HOSP_CODE處理欄位
                if (store.getAt(0).get('HOSP_CODE') == '804') {
                    // 若為804(桃園)則顯示庫備識別; 明細以顏色區分庫備/非庫備;隱藏[取消送申請]按鈕
                    storeidColorName = true;
                    setCancelHidden = true;
                    setVisibleColumns(true);
                }
                else {
                    storeidColorName = false;
                    setCancelHidden = false;
                    setVisibleColumns(false);
                }

                if (store.getAt(0).get('HOSP_CODE') == '803') {
                    // 若為803(台中)則顯示HIS
                    T2Grid.down('#import_his14_supdet').show();
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 設定欄位是否顯示
    var setVisibleColumns = function (optVal) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        T1Grid.suspendLayouts();
        for (var i = 1; i < T1Grid.columns.length; i++) {
            if (T1Grid.columns[i].showOption == 'chk') {
                if (optVal == false)
                    T1Grid.columns[i].setVisible(false);
                else
                    T1Grid.columns[i].setVisible(true);
            }

        }
        T1Grid.resumeLayouts(true);

        T2Grid.suspendLayouts();
        for (var i = 1; i < T2Grid.columns.length; i++) {
            if (T2Grid.columns[i].showOption == 'chk') {
                if (optVal == false)
                    T2Grid.columns[i].setVisible(false);
                else
                    T2Grid.columns[i].setVisible(true);
            }
        }
        T2Grid.resumeLayouts(true);

        if (setCancelHidden == true) {
            T1Grid.down('#cancel').hide();
        } else {
            T1Grid.down('#cancel').show();
        }


        myMask.hide();
    }

    var st_iscontid3 = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetIscontid3Combo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_isarmy = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetIsArmyCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // -- flylon
    var st_combo_sectionno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetCombo科別',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    }); // end of st_combo_sectionno
    var st_combo_sectionno_2 = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetCombo科別_2',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    }); // end of st_combo_sectionno
    //var st_combo_mmcode = Ext.create('Ext.data.Store', {
    //    proxy: {
    //        type: 'ajax',
    //        actionMethods: {
    //            read: 'POST' // by default GET
    //        },
    //        url: '/api/AB0118/GetCombo院內碼',
    //        reader: {
    //            type: 'json',
    //            rootProperty: 'etts'
    //        }
    //    },
    //    autoLoad: true
    //}); // end of st_combo_mmcode    
    // -- flylon end 

    var T7QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'T7_MMCODE_OR_MMNAME_C',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 350,
        limit: 50, //限制一次最多顯示10筆
        queryUrl: '/api/AB0118/GetMMCodeCombo_2', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T8QueryMMCode_1 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 350,
        limit: 50, //限制一次最多顯示10筆
        queryUrl: '/api/AB0118/GetMMCodeCombo_2', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T8QueryMMCode_2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        allowBlank: false, // 欄位為必填
        fieldCls: 'required',
        limit: 50, //限制一次最多顯示10筆
        queryUrl: '/api/AB0118/GetMMCodeCombo_3', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    // 查詢欄位


    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
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
                fieldLabel: '申請單',
                name: 'P0',
                id: 'P0'
            }, {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D0',
                id: 'D0',
                vtype: 'dateRange',
                dateRange: { end: 'D1' },
                padding: '0 4 0 4'
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'D1',
                id: 'D1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'D0' },
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                fieldLabel: '申請單狀態',
                name: 'P2',
                id: 'P2',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P4',
                id: 'P4',
                store: st_matclass,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                var f = this.up('form').getForm();
                if (f.findField('P0').getValue() == '' &&
                    f.findField('D0').rawValue == '' &&
                    f.findField('D1').rawValue == '' &&
                    f.findField('P2').getValue() == '' &&
                    //f.findField('P3').getForm() == '' &&
                    f.findField('P4').getValue() == '') {
                    Ext.Msg.alert('提醒', '至少需輸入1個條件');
                }
                else {
                    T1Load();
                }
                msglabel('訊息區:');
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
                T1QueryForm.getForm().findField('P2').setValue(arrP2);
            }
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'DOCTYPE', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'USEID', type: 'string' },
            { name: 'USEDEPT', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'PURCHASENO', type: 'string' },
            { name: 'SUPPLYNO', type: 'string' },
            { name: 'APPLY_KIND', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'MAT_CLASS_N', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'APPLY_KIND_N', type: 'string' },
            { name: 'APP_NAME', type: 'string' },
            { name: 'APPDEPT_NAME', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'EXT', type: 'string' },
            { name: 'APP_AMOUT', type: 'string' },
            { name: 'ISCONTID3', type: 'string' },
            { name: 'SRCDOCNO', type: 'string' },
            { name: 'ISARMY', type: 'string' },
            { name: 'ISARMY_N', type: 'string' },
            { name: 'APPUNA', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_CONTID_T', type: 'string' },
            { name: 'M_STOREID_T', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/AllM',
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
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    d0: T1QueryForm.getForm().findField('D0').rawValue,
                    d1: T1QueryForm.getForm().findField('D1').rawValue,
                    p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: '',
                    p4: T1QueryForm.getForm().findField('P4').getValue()
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
        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    function printAfterApply(docnos) {
        var docnoStr = '';
        var docnoStr2 = '';

        $.each(docnos, function (index, item) {
            docnoStr += '<br>' + item;
            docnoStr2 += item + ',';
        });


        Ext.Msg.show({
            title: '列印申請單',
            message: '是否列印申請單? <br> 單號:' + docnoStr,
            buttons: Ext.Msg.YESNO,
            buttonText: {
                yes: '是',
                no: '否'
            },
            icon: Ext.Msg.QUESTION,
            fn: function (btn) {
                if (btn === 'yes') {
                    showReport(docnoStr2);
                }
            }
        });
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', handler: function () {
                    T1Set = '/api/AB0118/CreateM';
                    // 取得每日單號 select GET_DAILY_DOCNO from DUAL 
                    // 依庫房代碼(WH_NO)查庫房基本檔 是否申請庫房已作廢 select 1 from MI_WHMAST where cancel_id = 'N' and wh_no = :towh 
                    // SELECT WH_NO 
                    // FROM MI_WHMAST  
                    // WHERE 1=1 
                    // AND WH_KIND = '1' // 庫別分類(0藥品庫 1衛材庫 E能設 C通信)
                    // AND WH_GRADE = '1' //庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍)
                    // and cancel_id = 'N' // 是否作廢
                    // AND ROWNUM=1
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0118/UpdateM';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
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
                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0118/DeleteM',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.MessageBox.alert('訊息', '刪除申請單號<br>' + name + '成功');
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:資料刪除成功');
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        }
                        );
                    }
                }
            },
            {
                itemId: 'apply', text: '提出申請', disabled: true, handler: function () {
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

                        //20230913增加 桃園需求:檢查出貨單位但不卡死
                        Ext.Ajax.request({
                            url: '/api/AB0118/CheckUnitrateFlg',
                            method: reqVal_p,
                            params: {
                                DOCNO: docno
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    if (data.msg != '') {
                                        Ext.MessageBox.confirm('提醒', '申請單號(' + data.msg + ')有品項申請量不為出貨單位的倍數，是否仍要提出申請?', function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/AB0118/Apply',
                                                    method: reqVal_p,
                                                    params: {
                                                        DOCNO: docno
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            T2Store.removeAll();
                                                            T1Grid.getSelectionModel().deselectAll();
                                                            T1QueryForm.getForm().findField('P0').setValue(''); //提出申請後單號清空才查詢
                                                            T1Load();
                                                            msglabel('訊息區:提出申請成功');
                                                            printAfterApply(data.etts);
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
                                    } else {
                                        Ext.MessageBox.confirm('提出申請', '是否確定提出申請？單號如下<br>' + name, function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/AB0118/Apply',
                                                    method: reqVal_p,
                                                    params: {
                                                        DOCNO: docno
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            T2Store.removeAll();
                                                            T1Grid.getSelectionModel().deselectAll();
                                                            T1QueryForm.getForm().findField('P0').setValue(''); //提出申請後單號清空才查詢
                                                            T1Load();
                                                            msglabel('訊息區:提出申請成功');
                                                            printAfterApply(data.etts);
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
                                else
                                    Ext.MessageBox.alert('錯誤', data.msg);
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });


                    }
                }
            },
            {
                itemId: 'cancel', text: '取消送申請', disabled: true, handler: function () {
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
                                    url: '/api/AB0118/Cancel',
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
                                            msglabel('訊息區:取消送申請申請成功');
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
            },
            {
                itemId: 'savepk', text: '套餐儲存', disabled: true, hidden: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin3();
                }
            },
            {
                itemId: 'print', text: '列印', disabled: true, handler: function () {
                    if (T2Store.data.length > 0)
                        showReport(T1LastRec.data.DOCNO);
                    else
                        Ext.Msg.alert('訊息', '此申請單尚無明細品項');
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM'); // /Scripts/app/model/ME_DOCM.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("DOCNO");
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('DOCNO_D').setValue('系統自編');
            f.findField('FLOWID_N').setValue('申請中');
            f.findField('APPTIME_T').setValue(st_getlogininfo.getAt(0).get('TODAY'));
            f.findField('FRWH_N').setValue(st_getlogininfo.getAt(0).get('CENTER_WHNAME'));
            f.findField('APP_NAME').setValue(st_getlogininfo.getAt(0).get('USERNAME'));
            f.findField('APPDEPT_NAME').setValue(st_getlogininfo.getAt(0).get('INIDNAME'));
            f.findField('FLOWID').setValue('1');
            f.findField('DOCTYPE').setValue('MR5');
            f.findField('ISARMY').setValue(st_isarmy.getAt(1).get('VALUE'));
            if (st_towhcombo.getCount() > 0) {
                f.findField('TOWH').setValue(st_towhcombo.getAt(0).get('VALUE'));
            }
            else {
                viewport.down('#form').collapse();
                Ext.MessageBox.alert('錯誤', '查無庫房資料,不得新增');
            }
            //北投需求 預設 02衛材
            if (st_matclass.getCount() > 0) {
                f.findField('MAT_CLASS').setValue(st_matclass.getAt(0).get('VALUE'));
            }
            //北投需求 預設 是
            if (hosp_code == '818') {
                f.findField('ISCONTID3').setValue(st_iscontid3.getAt(1).get('VALUE'));
            } else {
                f.findField('ISCONTID3').setValue(st_iscontid3.getAt(0).get('VALUE'));
            }

            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField('TOWH').setReadOnly(false);
            f.findField('ISCONTID3').setReadOnly(false);
        }
        else {
            u = f.findField('DOCNO');
        }
        f.findField('x').setValue(x);
        f.findField('ISARMY').setReadOnly(false);
        f.findField('APPUNA').setReadOnly(false);
        f.findField('APPLY_NOTE').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
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
        }],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 180
            }, {
                text: "狀態",
                dataIndex: 'FLOWID_N',
                width: 80
            }, {
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 100
            }, {
                text: "申請日期",
                dataIndex: 'APPTIME_T',
                width: 100
            }, {
                text: "出庫庫房",
                dataIndex: 'FRWH_N',
                width: 120
            }, {
                text: "入庫庫房",
                dataIndex: 'TOWH_N',
                width: 120
            }, {
                text: "物料分類",
                dataIndex: 'MAT_CLASS_N',
                width: 100
            }, {
                text: "是否小額採購",
                dataIndex: 'ISCONTID3',
                width: 100
            }, {
                text: "軍民別",
                dataIndex: 'ISARMY_N',
                width: 100
            }, {
                text: "申請人姓名",
                dataIndex: 'APPUNA',
                width: 100
            }, {
                text: "合約識別碼",
                dataIndex: 'M_CONTID_T',
                width: 100
            }, {
                text: "庫備識別碼",
                dataIndex: 'M_STOREID_T',
                width: 100,
                showOption: 'chk',
                hidden: true
            }, {
                xtype: 'hidden', // textfield 入庫庫房(TOWH)
                name: 'TOWH'
            }, {
                header: "",
                flex: 1
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
                viewport.down('#form').expand();
                T1Rec = records.length;
                T1LastRec = records[0];
                var GridRecords = T1Grid.getSelection();
                if (GridRecords.length > 1) {
                    T2Grid.down('#add').setDisabled(true);
                    T2Grid.down('#edit').setDisabled(true);
                    T2Grid.down('#delete').setDisabled(true);
                    T2Grid.down('#getpk').setDisabled(true);
                    T2Grid.down('#getsave').setDisabled(true);
                    T2Grid.down('#getexport').setDisabled(true);
                    T2Grid.down('#detailExport').setDisabled(true);
                    T2Grid.down('#import_his14_supdet').setDisabled(true);
                    //T2Grid.down('#addMulti').setDisabled(true);
                    T1Grid.down('#edit').setDisabled(true);
                    T1Grid.down('#print').setDisabled(true);
                    T1Grid.down('#delete').setDisabled(false);
                    T1Grid.down('#apply').setDisabled(false);
                    T1Grid.down('#cancel').setDisabled(false);

                    for (var i = 0; i < GridRecords.length; i++) {
                        if (GridRecords[i].data.FLOWID != '1') {
                            T1Grid.down('#delete').setDisabled(true);
                            T1Grid.down('#apply').setDisabled(true);
                        }
                        if (GridRecords[i].data.FLOWID != '11') {
                            T1Grid.down('#cancel').setDisabled(true);
                        }
                        var hosp_code = st_getlogininfo.getAt(0).get('HOSP_CODE');
                        var user_inid = st_getlogininfo.getAt(0).get('INID');
                        var is_grade1 = st_getlogininfo.getAt(0).get('IS_GRADE1');
                        if (hosp_code != "818") {
                            if (is_grade1 == 'Y' && user_inid != GridRecords[i].data.APPDEPT) {
                                T1Grid.down('#delete').setDisabled(true);
                                T1Grid.down('#apply').setDisabled(true);
                                T1Grid.down('#cancel').setDisabled(true);
                            }
                        }
                    }
                }
                else {
                    setFormT1a();
                }

                if (T1Grid.getSelection().length == 1 && T1LastRec.data.DOCNO != null) {
                    T2Grid.down('#detailExport').setDisabled(false);
                }
                else {
                    T1Grid.down('#print').setDisabled(true);
                }
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T1Grid.down('#cancel').setDisabled(T1Rec === 0);
        T1Grid.down('#savepk').setDisabled(T1Rec === 0);
        T1Grid.down('#print').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        T2Grid.down('#getpk').setDisabled(T1Rec === 0);
        T2Grid.down('#getsave').setDisabled(T1Rec === 0);
        T2Grid.down('#getexport').setDisabled(T1Rec === 0);
        T2Grid.down('#detailExport').setDisabled(T1Rec === 0);
        T2Grid.down('#import_his14_supdet').setDisabled(T1Rec === 0); // 匯入HIS骨科衛材
        //T2Grid.down('#addMulti').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            T3Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('DOCNO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('MAT_CLASS').setReadOnly(true);
            f.findField('TOWH').setReadOnly(true);
            T1F1 = f.findField('DOCNO').getValue();
            f.findField('DOCNO_D').setValue(T1F1);
            T1F2 = f.findField('MAT_CLASS').getValue();
            T1F3 = f.findField('FLOWID').getValue();
            T1F4 = f.findField('TOWH').getValue();
            T1F5 = f.findField('APPDEPT').getValue();
            T1F6 = f.findField('APPDEPT_NAME').getValue();
            T1F7 = f.findField('MAT_CLASS_N').getValue();
            T1F8 = f.findField('DOCTYPE').getValue();
            T1F9 = f.findField('ISCONTID3').getValue();
            T3Form.getForm().findField('NOTE').setValue(T1F6 + T1F7 + '套餐');
            st_pkdocno.load();
            st_pknote.load();
            debugger;
            if (T1F3 == '1' && getInidChk()) {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#delete').setDisabled(false);
                T1Grid.down('#apply').setDisabled(false);
                T1Grid.down('#print').setDisabled(true);
                T2Grid.down('#add').setDisabled(false);
                // T2Grid.down('#addMulti').setDisabled(false);
                T2Grid.down('#getpk').setDisabled(false);
                T2Grid.down('#getsave').setDisabled(false);
                T2Grid.down('#getexport').setDisabled(false);
                T2Grid.down('#detailExport').setDisabled(false);
                T2Grid.down('#import_his14_supdet').setDisabled(false); // 匯入HIS骨科衛材
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#delete').setDisabled(true);
                T1Grid.down('#apply').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
                //T2Grid.down('#addMulti').setDisabled(true);
                T2Grid.down('#getpk').setDisabled(true);
                T2Grid.down('#getsave').setDisabled(true);
                T2Grid.down('#getexport').setDisabled(true);
                T2Grid.down('#detailExport').setDisabled(true);
                T2Grid.down('#import_his14_supdet').setDisabled(true); // 匯入HIS骨科衛材
                if (T1F3 == '1') {
                    T1Grid.down('#print').setDisabled(true);
                }
                else {
                    T1Grid.down('#print').setDisabled(false);
                }
            }
            if (T1F3 == '11' && getInidChk()) {
                T1Grid.down('#cancel').setDisabled(false);
            }
            else {
                T1Grid.down('#cancel').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
        }
        T2Cleanup();
        T2Query.getForm().reset();
        T2Load(true);
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        hidden: true,
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
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'DOCTYPE',
            xtype: 'hidden'
        }, {
            name: 'FLOWID',
            xtype: 'hidden'
        }, {
            name: 'FRWH',
            xtype: 'hidden'
        }, {
            name: 'APPID',
            xtype: 'hidden'
        }, {
            name: 'APPDEPT',
            xtype: 'hidden'
        }, {
            name: 'USEID',
            xtype: 'hidden'
        }, {
            name: 'USEDEPT',
            xtype: 'hidden'
        }, {
            name: 'APPTIME',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'MAT_CLASS_N',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請單號',
            name: 'DOCNO_D'
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態',
            name: 'FLOWID_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請人員',
            name: 'APP_NAME'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請部門',
            name: 'APPDEPT_NAME'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請日期',
            name: 'APPTIME_T'
        }, {
            xtype: 'displayfield',
            fieldLabel: '出庫庫房',
            name: 'FRWH_N'

        }, {
            xtype: 'combo',
            fieldLabel: '入庫庫房',
            name: 'TOWH',
            store: st_towhcombo,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }, {
            xtype: 'combo',
            fieldLabel: '物料分類',
            name: 'MAT_CLASS',
            store: st_matclass,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }, {
            xtype: 'combo',
            fieldLabel: '是否小額採購',
            name: 'ISCONTID3',
            store: st_iscontid3,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }, {
            xtype: 'combo',
            fieldLabel: '軍民別',
            name: 'ISARMY',
            store: st_isarmy,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }, {
            xtype: 'textfield',
            fieldLabel: '申請人姓名',
            name: 'APPUNA',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APPLY_NOTE',
            enforceMaxLength: true,
            maxLength: 100,
            //height: 200,
            readOnly: true
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            if (T1Form.getForm().findField('ISARMY').getValue() == '1') { //選擇軍款要提醒2次
                                Ext.MessageBox.confirm(confirmSubmit, '選擇軍款，是否確定?', function (btn, text) {
                                    if (btn === 'yes') {
                                        Ext.MessageBox.confirm(confirmSubmit, '選擇軍款，是否確定?', function (btn, text) {
                                            if (btn === 'yes') {
                                                T1Submit();
                                            }
                                        });
                                    }
                                });
                            } else {
                                T1Submit();
                            }
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            //var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1QueryForm.getForm().reset();
                            var v = action.result.etts[0];
                            T1QueryForm.getForm().findField('P0').setValue(v.DOCNO);
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料提出申請成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T1Cleanup();
                    T1Load();
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
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
        T2Cleanup();
        TATabs.setActiveTab('Query');
        T1QueryForm.getForm().findField('P2').setValue(arrP2);
    }

    function showReport(docno) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?docno=' + docno + '&Order=mmcode" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0118/GetMMCodeDocd', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('DOCNO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [
            T2QueryMMCode,
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    T2Load(true);
                }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus();
                }
            }, {
                // 2023.03.25依建議不顯示申請總金額
                xtype: 'displayfield',
                fieldLabel: '申請總金額',
                padding: '0 0 0 8',
                labelAlign: 'right',
                name: 'APP_AMOUT',
                labelWidth: 70,
                hidden: true,
                renderer: function (val, meta, record) {
                    if (val != undefined) {
                        return '<span style="color:red">' + val + '</span>';
                    }
                }
            }
        ]
    });


    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'FRWH_D', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'STAT', type: 'string' },
            { name: 'RSEQ', type: 'string' },
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'PICK_QTY', type: 'string' },
            { name: 'PICK_USER', type: 'string' },
            { name: 'PICK_TIME', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'AVG_APLQTY', type: 'string' },
            { name: 'APP_AMT', type: 'string' },
            { name: 'HIGH_QTY', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'SRCDOCNO', type: 'string' },
            { name: 'S_INV_QTY', type: 'string' },
            { name: 'CHINNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_CONTID_T', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            { name: 'M_STOREID_T', type: 'string' },
            { name: 'CHINNAME_OLD', type: 'string' },
            { name: 'CHARTNO_OLD', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                //if (successful) 
                //    reCalAppAmout();
            }
        }
    });
    function T2Load(loadFirst) {
        try {
            if (loadFirst) {
                T2Tool.moveFirst();
            } else {
                T2Store.load({
                    params: {
                        start: 0
                    }
                });
            }

        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
        //viewport.down('#form').collapse();
    }
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
        width: 220,
        matchFieldWidth: false,
        listConfig: { width: 180 },
        margin: '0 0 10 0',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0118/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'M_CONTPRICE', 'AVG_PRICE', 'AVG_APLQTY', 'HIGH_QTY', 'TOT_APVQTY', 'PFILE_ID', 'S_INV_QTY', 'M_AGENNO', 'INV_QTY', 'APPQTY_TIMES', 'UNITRATE'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('MAT_CLASS').getValue(),
                p2: T1Form.getForm().findField('TOWH').getValue(),
                p3: T1Form.getForm().findField('DOCNO').getValue(),
                p4: T1Form.getForm().findField('FRWH').getValue(),
                p5: T1Form.getForm().findField('ISCONTID3').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('M_CONTPRICE').setValue(r.get('M_CONTPRICE'));
                T2Form.getForm().findField('AVG_PRICE').setValue(r.get('AVG_PRICE'));
                T2Form.getForm().findField('AVG_APLQTY').setValue(r.get('AVG_APLQTY'));
                T2Form.getForm().findField('HIGH_QTY').setValue(r.get('HIGH_QTY'));
                //T2Form.getForm().findField('TOT_APVQTY').setValue(r.get('TOT_APVQTY'));
                T2Form.getForm().findField('PFILE_ID').setValue(r.get('PFILE_ID'));

                if (r.get('WHMM_VALID') == 'Y') {
                    Ext.getCmp('WHMM_VALID').hide();
                } else {
                    Ext.getCmp('WHMM_VALID').show();
                }

                if (T2Form.getForm().findField('APPQTY').getValue()) {
                    // 申請金額 = 庫存平均單價 * 申請數量
                    var parDISC_UPRICE = parseFloat(T2Form.getForm().findField('DISC_UPRICE').getValue());
                    var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                    T2Form.getForm().findField('APP_AMT').setValue(accMul(parDISC_UPRICE, parAPPQTY));
                }
                T2Form.getForm().findField('S_INV_QTY').setValue(r.get('S_INV_QTY'));
                T2Form.getForm().findField('M_AGENNO').setValue(r.get('M_AGENNO'));
                T2Form.getForm().findField('INV_QTY').setValue(r.get('INV_QTY'));
                T2Form.getForm().findField('APPQTY_TIMES').setValue(r.get('APPQTY_TIMES'));
                T2Form.getForm().findField('UNITRATE').setValue(r.get('UNITRATE'));
            }
        }
    });
    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        autoScroll: true,
        cls: 'T2b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 110,
            msgTarget: 'side'
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
            name: 'SRCDOCNO',
            xtype: 'hidden'
        }, {
            name: 'SEQ',
            xtype: 'hidden'
        }, {
            name: 'STAT',
            xtype: 'hidden'
        },
        {
            xtype: 'container',
            layout: 'hbox',
            //padding: '0 7 7 0',
            items: [
                T2FormMMCode,
                {
                    xtype: 'button',
                    itemId: 'btnMmcode',
                    iconCls: 'TRASearch',
                    handler: function () {
                        var f = T2Form.getForm();
                        if (!f.findField("MMCODE").readOnly) {
                            popMmcodeForm_14(viewport, '/api/AB0118/GetMmcode', {
                                MMCODE: f.findField("MMCODE").getValue(),
                                MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
                                WH_NO: T1Form.getForm().findField('TOWH').getValue(),
                                ISCONTID3: T1Form.getForm().findField('ISCONTID3').getValue()
                            }, setMmcode);
                        }
                    }
                },

            ]
        }, {
            xtype: 'displayfield',
            value: '<spam style="color:red">本庫房無法申領此院內碼</span>',
            name: 'WHMM_VALID',
            id: 'WHMM_VALID',
            hidden: true,
        }, {
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
            xtype: 'displayfield',
            fieldLabel: '供應廠商',
            name: 'M_AGENNO'
        }, {
            xtype: 'displayfield',
            fieldLabel: '單價',
            name: 'M_CONTPRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '優惠最小單價',
            name: 'DISC_UPRICE',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '平均單價',
            name: 'AVG_PRICE',
            submitValue: true,
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '平均申請數量',
            name: 'AVG_APLQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請金額',
            name: 'APP_AMT',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請倍數',
            name: 'APPQTY_TIMES'
        }, {
            fieldLabel: '申請數量',
            name: 'APPQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function () {
                    // 申請金額 = 庫存平均單價 * 申請數量
                    var parDISC_UPRICE = parseFloat(T2Form.getForm().findField('DISC_UPRICE').getValue());
                    var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                    T2Form.getForm().findField('APP_AMT').setValue(accMul(parDISC_UPRICE, parAPPQTY));
                }
            }
        }, {
            xtype: 'label',
            text: '申請量必須符合申請倍數',
            style: 'color: #ff0000;',
            margin: '0 0 10 0'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫房存量',
            name: 'INV_QTY',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '基準量',
            name: 'HIGH_QTY',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '上級庫庫房存量',
            name: 'S_INV_QTY',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '出貨單位',
            name: 'UNITRATE'
        }, {
            xtype: 'combo',
            fieldLabel: '超量原因',
            name: 'GTAPL_RESON',
            store: st_reason,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            readOnly: true,
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            xtype: 'hidden'
        }, {
            xtype: 'textfield',
            fieldLabel: '病患姓名',
            name: 'CHINNAME',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true
        }, {
            xtype: 'textfield',
            fieldLabel: '病歷號',
            name: 'CHARTNO',
            enforceMaxLength: true,
            maxLength: 10,
            readOnly: true
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APLYITEM_NOTE',
            enforceMaxLength: true,
            maxLength: 50,
            height: 200,
            readOnly: true
        }, {
            xtype: 'imagegrid',
            fieldLabel: '附加圖片',
            name: 'PFILE_ID',
            width: 300,
            height: 200,
            hidden: true
        }, {
            name: 'CHINNAME_OLD',
            xtype: 'hidden'
        }, {
            name: 'CHARTNO_OLD',
            xtype: 'hidden'
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                var isSub = true;
                if (this.up('form').getForm().findField('APPQTY').getValue() == '0') {
                    Ext.Msg.alert('提醒', '申請數量不可為0');
                    isSub = false;
                }
                else {
                    var highqty = 9999;
                    if (this.up('form').getForm().findField('HIGH_QTY').getValue() != null) {
                        highqty = Number(this.up('form').getForm().findField('HIGH_QTY').getValue());
                    }
                    //var tot_apvqty = 0;
                    //if (this.up('form').getForm().findField('TOT_APVQTY').getValue() != null) {
                    //    tot_apvqty = Number(this.up('form').getForm().findField('TOT_APVQTY').getValue());
                    //}
                    var appqty = 0;
                    if (this.up('form').getForm().findField('APPQTY').getValue() != null) {
                        appqty = Number(this.up('form').getForm().findField('APPQTY').getValue());
                    }
                    var appqty_times = 0;
                    if (this.up('form').getForm().findField('APPQTY_TIMES').getValue() != null) {
                        appqty_times = Number(this.up('form').getForm().findField('APPQTY_TIMES').getValue());
                    }
                    var unitrate = 0;
                    if (this.up('form').getForm().findField('UNITRATE').getValue() != null) {
                        unitrate = Number(this.up('form').getForm().findField('UNITRATE').getValue());
                    }
                    //if ((tot_apvqty + appqty) > highqty && this.up('form').getForm().findField('GTAPL_RESON').getValue() == null) {
                    //    Ext.Msg.alert('提醒', '本月累計核撥量加本次申請量超過月基準量，請敘明原因!');
                    //    isSub = false;
                    //}
                    if ((appqty % appqty_times) != 0) {
                        Ext.Msg.alert('提醒', '申請量必須符合申請倍數!');
                        isSub = false;
                    }
                    //var invqty = Number(this.up('form').getForm().findField('INV_QTY').getValue())
                    if (appqty > highqty) {
                        Ext.Msg.alert('提醒', '申請量不可超過基準量(庫房設定的單位請領量)');
                        isSub = false;
                    }
                }
                if (isSub) {
                    //20230913增加 桃園需求:檢查出貨單位但不卡死
                    if ((appqty % unitrate) != 0) {
                        Ext.MessageBox.confirm('提醒', '申請量不為出貨單位的倍數，是否仍要儲存?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        });
                    } else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        });
                    }
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: function () {
                T2Cleanup();
                T2Load(true);
                viewport.down('#form').collapse();
            }
        }]
    });
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            //var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
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
                            //reCalAppAmout();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            //reCalAppAmout();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    if (f2.findField("x").getValue() == "I") {
                        T11Set = '../../../api/AB0118/CreateD';
                        setFormT2('I', '新增');
                    }
                    else {

                        if (f2.findField("x").getValue() == "U") {
                            T2Load(false);
                        } else {
                            T2Load(true);
                        }
                        T2Cleanup();
                    }

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
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        f.findField('MMCODE').setReadOnly(true);
        f.findField('APPQTY').setReadOnly(true);
        f.findField('CHINNAME').setReadOnly(true);
        f.findField('CHARTNO').setReadOnly(true);
        f.findField('APLYITEM_NOTE').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        T2Form.down('#btnMmcode').setVisible(false);
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
                    T11Set = '../../../api/AB0118/CreateD';
                    Ext.getCmp('WHMM_VALID').hide();
                    setFormT2('I', '新增');
                },
                hidden: menuLink == 'AB0142' ? true : false
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '../../../api/AB0118/UpdateD';
                    Ext.getCmp('WHMM_VALID').hide();
                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        let seq = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('SEQ') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0118/DeleteD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load(true);
                                        } else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
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
            {
                itemId: 'getpk', text: '套餐轉入', disabled: true, hidden: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin4();
                }
            },
            {
                itemId: 'getsave', text: '低於安全存量轉入', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin5();
                }
            },
            {
                itemId: 'getexport', text: '匯入', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    showWin6();
                }
            },
            {
                itemId: 'detailExport', id: 'detailExport', text: '匯出', disabled: true, handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: T1LastRec.data.DOCNO });
                    PostForm(detailExport, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                itemId: 'import_his14_supdet', id: 'import_his14_supdet', text: '匯入HIS骨科衛材',
                disabled: true,
                hidden: true,
                handler: function () {
                    //HIS資料最後轉入時間
                    Ext.Ajax.request({
                        url: '/api/AB0118/GetLastHIS14SUPDET',
                        method: reqVal_p,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg != "" && data.msg != null) {
                                    LastHIS14SUPDET = data.msg.substring(0, 3) + "/" + data.msg.substring(3, 5) + "/" + data.msg.substring(5, 7) + " " + data.msg.substring(7, 9) + ":" + data.msg.substring(9, 11);
                                }
                                Ext.getCmp('T7_LastHIS14SUPDET').setValue(' <span style=\'color:red\'>' + LastHIS14SUPDET + '</span>');
                                // 0.進入[匯入HIS骨科衛材]彈跳視窗，清空login user暫存區的資料
                                Ext.Ajax.request({
                                    url: '/api/AB0118/Clear_Temp_HIS14_SUPDET',
                                    method: reqVal_p,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            showWin7();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }
        ]
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T2Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f2.reset();
            //var r = Ext.create('T2Model');
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            f2.findField('SRCDOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");
            f2.findField('MMCODE').setReadOnly(false);
            T2Form.down('#btnMmcode').setVisible(true);

            //u.setReadOnly(false);
        }
        else {
            u = f2.findField('APPQTY');
        }

        f2.findField('x').setValue(x);
        f2.findField('STAT').setValue('1');
        f2.findField('APPQTY').setReadOnly(false);
        f2.findField('GTAPL_RESON').setReadOnly(false);
        f2.findField('CHINNAME').setReadOnly(false);
        f2.findField('CHARTNO').setReadOnly(false);
        f2.findField('APLYITEM_NOTE').setReadOnly(false);
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
            layout: 'fit',
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true,
            renderer: function (val, meta, record) {
                if (storeidColorName == true) { // 庫備,非庫備以不同顏色顯示 
                    if (record.data['M_STOREID'] == '0')
                        return "<font color='blue'>" + val + "</font>";
                    else if (record.data['M_STOREID'] == '1')
                        return "<font color='green'>" + val + "</font>";
                    else
                        return val;
                }
                else
                    return val;
            }
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
            sortable: true,
            renderer: function (val, meta, record) {
                if (storeidColorName == true) { // 庫備,非庫備以不同顏色顯示 
                    if (record.data['M_STOREID'] == '0')
                        return "<font color='blue'>" + val + "</font>";
                    else if (record.data['M_STOREID'] == '1')
                        return "<font color='green'>" + val + "</font>";
                    else
                        return val;
                }
                else
                    return val;
            }
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "上級庫庫房存量",
            dataIndex: 'S_INV_QTY',
            width: 120,
            sortable: true
        }, {
            text: "基準量",
            dataIndex: 'HIGH_QTY',
            width: 80,
            sortable: true
        }, {
            text: "單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            width: 100, align: 'right',
            xtype: 'hidden',
        }, {
            text: "申請金額",
            dataIndex: 'APP_AMT',
            style: 'text-align:left',
            width: 140, align: 'right',
            xtype: 'hidden',
            renderer: function (val, meta, record) {
                // 申請金額 = 庫存平均單價 * 申請數量
                var parDISC_UPRICE = parseFloat(record.data['DISC_UPRICE']);
                var parAPPQTY = parseFloat(record.data['APPQTY']);
                return accMul(parDISC_UPRICE, parAPPQTY);
            }
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "核撥量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "申請確認時間",
            dataIndex: 'APL_CONTIME',
            width: 100
        }, {
            text: "出庫庫房",
            dataIndex: 'FRWH_D',
            width: 70,
        }, {
            text: "病患姓名",
            dataIndex: 'CHINNAME',
            width: 80
        }, {
            text: "病歷號",
            dataIndex: 'CHARTNO',
            width: 80
        }, {
            text: "備註",
            dataIndex: 'APLYITEM_NOTE',
            width: 300
        }, {
            text: "合約識別碼",
            dataIndex: 'M_CONTID_T',
            width: 100
        }, {
            text: "庫備識別碼",
            dataIndex: 'M_STOREID_T',
            width: 100,
            showOption: 'chk',
            hidden: true
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                        TATabs.setActiveTab('Form');
                    }
                }
            },

            selectionchange: function (model, records) {
                viewport.down('#form').expand();
                T2Rec = records.length;
                T2LastRec = records[0];

                //if (T2Grid.getSelection().length > 1) {
                //    T2Grid.down('#edit').setDisabled(true);
                //}

                setFormT2a();


                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT2a() {
        var selections = T2Grid.getSelection();
        if (selections.length > 0) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');

            if (T1F3 === '1' && getInidChk()) {
                console.log('in 1');
                T2Grid.down('#edit').setDisabled(false);
                T2Grid.down('#delete').setDisabled(false);
            }
            else {
                console.log('in 2');
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }

            if (selections.length > 1) {
                T2Grid.down('#edit').setDisabled(true);
            }
        }
        else {
            T2Grid.down('#edit').setDisabled(true);
            T2Grid.down('#delete').setDisabled(true);
            T2Form.getForm().reset();
        }
    }
    var T3Form = Ext.widget({
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
        autoScroll: true,
        items: [
            {
                xtype: 'textareafield',
                fieldLabel: '菜單說明',
                name: 'NOTE',
                enforceMaxLength: true,
                maxLength: 100,
                height: 200
            }
        ],
        buttons: [{
            itemId: 'T3Submit', text: '儲存', handler: function () {
                if (this.up('form').getForm().isValid()) {
                    Ext.MessageBox.confirm('儲存套餐', '是否確定儲存套餐?', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0118/Savepk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1F1,
                                    MAT_CLASS: T1F2,
                                    NOTE: T3Form.getForm().findField('NOTE').getValue()
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        hideWin3();
                                        Ext.MessageBox.alert('訊息', '套餐儲存成功');
                                        T2Store.removeAll();
                                        T1Grid.getSelectionModel().deselectAll();
                                        T1Load();
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    }
                    );

                }
            }
        },
        {
            itemId: 'cancel', text: '取消', handler: hideWin3
        }]
    });

    Ext.define('T4Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'STAT', type: 'string' },
            { name: 'RSEQ', type: 'string' },
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'PICK_QTY', type: 'string' },
            { name: 'PICK_USER', type: 'string' },
            { name: 'PICK_TIME', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'APPQTY_TIMES', type: 'string' }
        ]
    });
    var T4Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetPackD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T4Query.getForm().findField('P0').getValue(),
                    p1: T4Query.getForm().findField('P1').getValue(),
                    p2: T1F2,
                    p3: T1F4,
                    p4: T1F9
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T4Load() {
        T4Store.load({
            params: {
                start: 0
            }
        });
    }
    var T4Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [{
            xtype: 'combo',
            fieldLabel: '套餐單號',
            name: 'P0',
            store: st_pkdocno,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            labelAlign: 'right',
            listeners: {
                select: function (c, r, eo) {
                    Ext.getCmp('T4btn1').setDisabled(false);
                    Ext.getCmp('T4btn2').setDisabled(false);
                }
            }
        }, {
            xtype: 'combo',
            fieldLabel: '套餐說明',
            name: 'P1',
            store: st_pknote,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            labelAlign: 'right',
            width: '500px',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            anyMatch: true,
            listeners: {
                select: function (c, r, eo) {
                    Ext.getCmp('T4btn1').setDisabled(false);
                    Ext.getCmp('T4btn2').setDisabled(false);
                }
            }
        }, {
            xtype: 'button',
            text: '查詢',
            id: 'T4btn1',
            disabled: true,
            handler: function () {
                T4Load();
                msglabel('訊息區:');
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }, {
            xtype: 'button', text: '轉入',
            id: 'T4btn2',
            disabled: true,
            handler: function () {
                var selection = T4Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    var msg = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (selection[i].data.APPQTY % selection[i].data.APPQTY_TIMES != 0) {
                            if (msg != '') {
                                msg += '<br>';
                            }
                            msg += ('院內碼:' + selection[i].data.MMCODE + ' 申請量:' + selection[i].data.APPQTY + ' 申請倍數:' + selection[i].data.APPQTY_TIMES);
                        }
                    }
                    if (msg != '') {
                        msg = '<span style="color:red">申請量須符合申請倍數，請至套餐建置修改</span><br>' + msg;
                        Ext.Msg.alert('提醒', msg);
                        return;
                    }

                    let docno = '';
                    let mmcode = '';
                    let appqty = '';
                    //selection.map(item => {
                    //    name += '「' + item.get('MMCODE') + '」<br>';
                    //    docno += T1F1 + ',';
                    //    mmcode += item.get('MMCODE') + ',';
                    //    appqty += item.get('APPQTY') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('MMCODE') + '」<br>';
                        docno += T1F1 + ',';
                        mmcode += item.get('MMCODE') + ',';
                        appqty += item.get('APPQTY') + ',';
                    })
                    Ext.MessageBox.confirm('轉入', '是否確定轉入？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0118/InsFromPk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    MMCODE: mmcode,
                                    APPQTY: appqty
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        //Ext.MessageBox.alert('訊息', '轉入成功');
                                        msglabel('訊息區:轉入成功');
                                        hideWin4();
                                        //T2Store.removeAll();
                                        T2Grid.getSelectionModel().deselectAll();
                                        T2Load(true);
                                    }
                                    else {
                                        hideWin4();
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    hideWin4();
                                }
                            });
                        }
                    }
                    );
                }
            }
        }
        ]
    });

    function T4Cleanup() {
        T4Query.getForm().reset();
        T4Load();
        msglabel('訊息區:');
        Ext.getCmp('T4btn1').setDisabled(true);
        Ext.getCmp('T4btn2').setDisabled(true);
    }
    var T4Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T4Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T4Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "",
            dataIndex: 'WHMM_VALID',
            width: 100,
            renderer: function (val, meta, record) {
                if (record.data.WHMM_VALID == 'N') {
                    return '<span style="color: red">本庫房不可申領</span>';
                }
            },
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "申請倍數",
            dataIndex: 'APPQTY_TIMES',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            header: "",
            flex: 1
        }]
    });

    var T5Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/GetSaveD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p2: T1F2,
                    p3: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T5Load() {
        T5Store.load({
            params: {
                start: 0
            }
        });
    }
    var T5Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [{
            xtype: 'button', text: '轉入',
            id: 'T5btn2',
            handler: function () {
                var selection = T5Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    let docno = '';
                    let mmcode = '';
                    let appqty = '';
                    //selection.map(item => {
                    //    name += '「' + item.get('MMCODE') + '」<br>';
                    //    docno += T1F1 + ',';
                    //    mmcode += item.get('MMCODE') + ',';
                    //    appqty += item.get('APPQTY') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('MMCODE') + '」<br>';
                        docno += T1F1 + ',';
                        mmcode += item.get('MMCODE') + ',';
                        appqty += item.get('APPQTY') + ',';
                    })
                    Ext.MessageBox.confirm('轉入', '是否確定轉入？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0118/InsFromPk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    MMCODE: mmcode,
                                    APPQTY: appqty
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        //Ext.MessageBox.alert('訊息', '轉入成功');
                                        msglabel('訊息區:轉入成功');
                                        hideWin5();
                                        //T2Store.removeAll();
                                        T2Grid.getSelectionModel().deselectAll();
                                        T2Load(true);
                                    }
                                    else {
                                        hideWin5();
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    hideWin5();
                                }
                            });
                        }
                    }
                    );
                }
            }
        }
        ]
    });

    function T5Cleanup() {
        T5Query.getForm().reset();
        T5Load();
        msglabel('訊息區:');
    }
    var T5Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T5Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T5Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "",
            dataIndex: 'WHMM_VALID',
            width: 100,
            renderer: function (val, meta, record) {
                if (record.data.WHMM_VALID == 'N') {
                    return '<span style="color: red">本庫房不可申領</span>';
                }
            },
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            width: 80, align: 'right'
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "超量原因",
            dataIndex: 'GTAPL_RESON_N',
            style: 'text-align:left',
            width: 150, align: 'right'
        }, {
            header: "",
            flex: 1
        }
        ]
    });

    var T6Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            //url: '/api/AB0118/GetSaveD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p2: T1F2,
                    p3: T1F5
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T6Load() {
        T6Store.load({
            params: {
                start: 0
            }
        });
    }
    var T6Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 't6export',
                name: 'T6insert',
                text: '範本', handler: function () {
                    var p = new Array();
                    //p.push({ name: 'FN', value: today + '_新合約品項批次更新_物料主檔.xls' });
                    PostForm(T6GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'T6send',
                id: 'T6send',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('T6insert').setDisabled(true);
                        T6Store.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('T6send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            // var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            formData.append("matclass", T1F2);
                            formData.append("docno", T1F1);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AB0118/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        T6Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T6insert').setDisabled(true);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T6Store.loadData(data.etts, false);
                                        IsSend = true;
                                        T6Grid.columns[1].setVisible(true);
                                        //T1Grid.columns[2].setVisible(true);
                                        if (data.msg == "True") {
                                            Ext.getCmp('T6insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                        };
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        };
                                    }
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    Ext.getCmp('T6insert').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '更新',
                id: 'T6insert',
                name: 'T6insert',
                disabled: true,
                handler: function () {
                    // var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AB0118/Insert',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(T6Store.data.items, 'data')),
                            docno: T1F1
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");
                                }
                                Ext.getCmp('T6insert').setDisabled(true);
                                T6Store.removeAll();
                                T6Grid.columns[1].setVisible(false);
                                T2Load(true);
                            }
                            myMask.hide();
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
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });

                    hideWin6();
                }
            }
        ]
    });

    function T6Cleanup() {
        T6Query.getForm().reset();
        //T6Load();
        msglabel('訊息區:');
    }
    var T6Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T6Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T6Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        columns: [{
            xtype: 'rownumberer'
        },
        {
            dataIndex: 'CHECK_RESULT',
            hidden: true
        },
        {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 100,
            sortable: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        },
        {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        },
        {
            text: "備註",
            dataIndex: 'APLYITEM_NOTE',
            style: 'text-align:left',
            width: 80
        },
        {
            header: "",
            flex: 1
        }
        ]
    });

    //view 
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
            items: [T1QueryForm]
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
        items: [
            {
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
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
                    } // end of t1Grid items[object]
                ] // endo of t1Grid items[]
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
            } // end of viewport items
        ]
    });

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;

    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '套餐儲存',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }

    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }

    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
            //viewport.down('#form').collapse();
        }
    }

    function setFormT8a() {
        T8Grid.down('#T8Tool_btn_del').setDisabled(T8Rec === 0);

        win7.down('#TATabs7_Form_Form').setTitle('瀏覽');
        win7.down('#TATabs7_Form_Form').expand();
        TATabs8.setActiveTab('TATabs8_Form');
        if (T8LastRec) {
            isNew = false;
            TATabs8_Form.loadRecord(T8LastRec);
            var f = TATabs8_Form.getForm();
            f.findField('x').setValue('U');

            if (st_combo_sectionno.getCount() > 0) { // 科別
                f.findField('SECTIONNO').setValue(T8LastRec.data.SECTIONNO);
            }
            //if (st_combo_mmcode.getCount() > 0) { // 院內碼
            //    f.findField('MMCODE').setValue(T8LastRec.data.MMCODE);
            //}
        }
        else {
            TATabs8_Form.getForm().reset();
        }
    }

    function T8Cleanup() {
        win7.down('#T8Grid').unmask();
        var f = TATabs8_Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        TATabs8_Form.down('#TATabs8_Form_cancel').hide();
        TATabs8_Form.down('#TATabs8_Form_submit').hide();
        win7.down('#TATabs7_Form_Form').setTitle('瀏覽');

        TATabs8.setActiveTab('TATabs8_Form');
        win7.down('#TATabs7_Form_Form').collapse();
    }

    Ext.define('T8Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'SECTIONNO', type: 'string' }, // 科別
            { name: 'SECTIONNAME', type: 'string' }, // 科別名稱
            { name: 'MMCODE', type: 'string' }, // 院內碼
            { name: 'MMNAME_C', type: 'string' }, // 中文品名
            { name: 'MMNAME_E', type: 'string' }, // 英文品名
        ]
    });

    var T8Store = Ext.create('Ext.data.Store', {
        model: 'T8Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/AllT8Grid',
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
                    SECTIONNO: T8QueryForm.getForm().findField('SECTIONNO').getValue(),
                    MMCODE: T8QueryForm.getForm().findField('MMCODE').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T8Load() {
        T8Store.load({
            params: {
                start: 0
            }
        });
        T8Tool.moveFirst();
        win7.down('#TATabs7_Form_Form').collapse(); //viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T8Tool = Ext.create('Ext.PagingToolbar', {
        store: T8Store,
        displayInfo: true,
        border: false,
        plain: true,
        items: [
            {
                xtype: 'button',
                id: 'T8Tool_btn_add',
                // disabled: false,
                text: '新增',
                handler: function () {
                    T1Set = '/api/AB0118/Create_SEC_USEMM';
                    msglabel('訊息區:');
                    st_combo_sectionno_2.load();// 科別   st_pkdocno.load();
                    //st_combo_mmcode.load(); // 院內碼   st_pknote.load();                    
                    setFormT8('I', '新增');
                    TATabs8.setActiveTab('TATabs8_Query');
                }
            }, {
                xtype: 'button',  // 
                id: 'T8Tool_btn_del',
                disabled: true,
                text: '刪除',
                handler: function () {
                    var selection = T8Grid.getSelection(); //var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let sectionnos = ''; // 科別
                        let mmcodes = ''; // 院內碼
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('MMNAME_C') + '」<br>';
                            sectionnos += item.get('SECTIONNO') + ',';
                            mmcodes += item.get('MMCODE') + ',';
                        })
                        Ext.MessageBox.confirm('刪除', '是否確定刪除?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0118/Delete_SEC_USEMM',  // DeleteM
                                    method: reqVal_p,
                                    params: {
                                        SECTIONNOS: sectionnos,
                                        MMCODES: mmcodes,
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.MessageBox.alert('訊息', '刪除<br>' + name + '成功');
                                            T8Grid.getSelectionModel().deselectAll();
                                            T8Load();
                                            msglabel('訊息區:資料刪除成功');
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                }); // end of Ext.Ajax.request({
                            } // end of if (btn === 'yes') {
                        }); // end of Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                    } // end of if (selection.length ===0)
                } // end of handler: function()
            }
        ] // end of T8Tool items
    }); // end of var T8Tool = Ext.create('Ext.PagingToolbar


    // -- T8 
    var T8QueryForm = Ext.widget({
        itemId: 'T8QueryForm',
        xtype: 'form',
        layout: 'form',
        border: false,
        title: '',
        autoScroll: false,
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'container',  // 第一行
                layout: 'hbox',  // 水平布局
                padding: '0 0 5 0',
                items: [{
                    name: 'x',
                    xtype: 'hidden'
                }, {
                    xtype: 'combo',
                    fieldLabel: '科別',
                    name: 'SECTIONNO',
                    id: 'SECTIONNO',
                    store: st_combo_sectionno,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE'
                }, T8QueryMMCode_1,
                //{
                //xtype: 'combo',
                //fieldLabel: '院內碼',
                //name: 'MMCODE',
                //id: 'MMCODE',
                //store: st_combo_mmcode,
                //queryMode: 'local',
                //displayField: 'COMBITEM',
                //valueField: 'VALUE',
                //width: '500px'
                //},
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 0 0 10',
                    handler: function () {
                        T8Load();
                        win7.down('#TATabs7_Form_Form').setTitle('瀏覽');
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',  // 按鈕上顯示的文本
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('SECTIONNO').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                        T8Cleanup();
                    }
                }] // end of items
            }]
        }]
    });

    function T7Submit() {
        var f = T8Form.getForm();
        if (f.isValid()) {
            // var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T7Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T8Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T7QueryForm.getForm().reset();
                            var v = action.result.etts[0];
                            T7QueryForm.getForm().findField('P0').setValue(v.DOCNO);
                            r.set(v);
                            T7Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料提出申請成功');
                            break;
                        case "D":
                            T7Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T7Cleanup();
                    T7Load();
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
    function T7Cleanup() {
        viewport.down('#T7Grid').unmask();
        var f = T8Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T8Form.down('#cancel').hide();
        T8Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');

        T2Cleanup();
        TATabs.setActiveTab('Query');
        T7QueryForm.getForm().findField('P2').setValue(arrP2);
    }

    var T7QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 70,
            width: 230
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                id: 'Panel7P1',
                border: false,
                layout: 'hbox',
                padding: '0 0 5 0',
                items: [{
                    xtype: 'datefield',
                    fieldLabel: '消耗日期',
                    labelWidth: 110,
                    width: 210,
                    name: 'T7_SUP_USEDATE_S',
                    id: 'T7_SUP_USEDATE_S',
                    allowBlank: false,
                    fieldCls: 'required',
                    vtype: 'dateRange',
                    dateRange: { end: 'D1' },
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: 9,
                    width: 109,
                    name: 'T7_SUP_USEDATE_E',
                    id: 'T7_SUP_USEDATE_E',
                    allowBlank: false,
                    fieldCls: 'required',
                    vtype: 'dateRange',
                    dateRange: { begin: 'T7_SUP_USEDATE_S' },
                    padding: '0 4 0 4'
                }, T7QueryMMCode,
                    //{
                    //xtype: 'textfield',
                    //fieldLabel: '院內碼或中文品名',
                    //labelWidth: 120,
                    //width: 300,
                    //padding: '0 4 0 4',
                    //name: 'T7_MMCODE_OR_MMNAME_C',
                    //id: 'T7_MMCODE_OR_MMNAME_C'
                    //}

                ] // end of items
            }, {
                xtype: 'panel',
                id: 'Panel7P2',
                border: false,
                layout: 'hbox',
                padding: '0 0 5 0',
                items: [{
                    xtype: 'textfield',
                    fieldLabel: '病患證號或姓名',
                    labelWidth: 110,
                    width: 327,
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    name: 'T7_SUP_PATIDNO_OR_SUP_PATNAME',
                    id: 'T7_SUP_PATIDNO_OR_SUP_PATNAME'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '醫師代碼或姓名',
                    labelWidth: 120,
                    width: 300,
                    padding: '0 4 0 4',
                    name: 'T7_SUP_FEATOPID_OR_SUP_EMPNAME',
                    id: 'T7_SUP_FEATOPID_OR_SUP_EMPNAME'
                }, {
                    xtype: 'button',
                    text: '查詢',  // 按鈕上顯示的文本
                    handler: function () {
                        var f = this.up('form').getForm();
                        //if (
                        //    f.findField('T7_SUP_USEDATE_S').rawValue == '' && // 消耗日期'
                        //    f.findField('T7_SUP_USEDATE_E').rawValue == '' &&
                        //    f.findField('T7_MMCODE_OR_MMNAME_C').getValue() == '' && // 院內碼或中文品名 rawValue
                        //    f.findField('T7_SUP_PATIDNO_OR_SUP_PATNAME').getValue() == '' && // 病患證號或姓名
                        //    f.findField('T7_SUP_FEATOPID_OR_SUP_EMPNAME').getValue() == '' // 醫師代碼或姓名
                        //) {
                        //    Ext.Msg.alert('提醒', '至少需輸入1個條件');
                        //}
                        //else {
                        //    T7Load();
                        //}
                        if (f.isValid()) {
                            T7Load();
                        }
                        else {
                            Ext.MessageBox.alert('提示', '請輸入必填欄位');
                        }
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',  // 按鈕上顯示的文本
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.findField('T7_SUP_USEDATE_S').reset();
                        f.findField('T7_SUP_USEDATE_E').reset();
                        f.findField('T7_MMCODE_OR_MMNAME_C').reset();
                        f.findField('T7_SUP_PATIDNO_OR_SUP_PATNAME').reset();
                        f.findField('T7_SUP_FEATOPID_OR_SUP_EMPNAME').reset();

                        f.findField('T7_SUP_USEDATE_S').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }]
            }, {
                xtype: 'panel',
                id: 'Panel7P3',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'displayfield',
                    fieldLabel: ' <span style=\'color:red\'>HIS資料最後轉入時間</span>',
                    labelWidth: 140,
                    width: 280,
                    name: 'T7_LastHIS14SUPDET',
                    id: 'T7_LastHIS14SUPDET',
                    padding: '0 4 0 4'
                }]
            }]
        }]
    });

    Ext.define('T7Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' }, // 院內碼
            { name: 'MMNAME_C', type: 'string' }, // 中文品名
            { name: 'USEQTY', type: 'string' }, // 耗用量
            { name: 'SUP_UNIT', type: 'string' }, // 耗用單位
            { name: 'IDNO', type: 'string' }, // 病患證號
            { name: 'PATIENTNAME', type: 'string' }, // 病患姓名
            { name: 'SECTIONNO', type: 'string' }, // 科別
            { name: 'SECTIONNAME', type: 'string' }, // 科別名稱
            { name: 'DRID', type: 'string' }, // 醫師代碼
            { name: 'DRNAME', type: 'string' }, // 醫師姓名
            { name: 'USEDATE', type: 'string' }, // 消耗日期
            { name: 'PROCDATE', type: 'string' }, // 處理日期
            { name: 'DOCNO', type: 'string' }, // 申請單號
            { name: 'SUPDET_SEQ', type: 'string' }, // 流水號
            { name: 'SUP_USEDATE', type: 'string' }, // 消耗日期
            { name: 'SUP_SKDIACODE', type: 'string' }, // 院內碼
            { name: 'SUP_PATNAME', type: 'string' }, // 病患姓名
            { name: 'SUP_MEDNO', type: 'string' }, // 病歷號
        ]
    });

    var T7Store = Ext.create('Ext.data.Store', {
        model: 'T7Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMNAME_C', direction: '' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0118/AllT7Grid',
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
                    SUP_USEDATE_S: T7QueryForm.getForm().findField('T7_SUP_USEDATE_S').rawValue, // 消耗日期開始
                    SUP_USEDATE_E: T7QueryForm.getForm().findField('T7_SUP_USEDATE_E').rawValue, // 消耗日期結束
                    MMCODE_OR_MMNAME_C: T7QueryForm.getForm().findField('T7_MMCODE_OR_MMNAME_C').getValue(), // 院內碼或中文品名
                    SUP_PATIDNO_OR_SUP_PATNAME: T7QueryForm.getForm().findField('T7_SUP_PATIDNO_OR_SUP_PATNAME').getValue(), // 病患證號或姓名
                    SUP_FEATOPID_OR_SUP_EMPNAME: T7QueryForm.getForm().findField('T7_SUP_FEATOPID_OR_SUP_EMPNAME').getValue() // 醫師代碼或姓名
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T7Load() {
        T7Store.load({
            params: {
                start: 0
            }
        });
        T7Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T7Tool = Ext.create('Ext.PagingToolbar', {
        store: T7Store,
        displayInfo: true,
        border: false,
        plain: true,
        items: [{
            xtype: 'button',  // 添加按钮组件
            id: 'T7Tool_btn_01',
            disabled: false,
            text: '轉入暫存區',
            handler: function () {
                if (T7Store.data.items.length == 0) {
                    Ext.Msg.alert('提醒', '請先查詢欲匯入HIS資料!');
                } else {
                    //(1)DOCNO有值
                    var sMsg = "";
                    var supdet_seq = "";
                    for (var i = 0; i < T7Store.data.items.length; i++) {
                        var o = T7Store.data.items[i].data;
                        if (o.DOCNO != "") {
                            sMsg += "消耗日期=" + o.USEDATE + ","; // SUP_USEDATE, 
                            sMsg += "院內碼=" + o.SUP_SKDIACODE + ", "; // SUP_SKDIACODE
                            sMsg += "病患姓名=" + o.PATIENTNAME + ", "; // SUP_PATNAME,
                            sMsg += "病歷號=" + o.SUP_MEDNO + " ";
                            sMsg += "已轉入請領單，申請單號=" + o.DOCNO;
                            sMsg += "<BR>\r\n";
                        } else {
                            supdet_seq += o.SUPDET_SEQ + ',';
                        }
                    }
                    if (sMsg.length > 0) {
                        sMsg += "，不可再轉申請單";
                        Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">" + sMsg + "</span>");
                        return;
                    }
                    //(2)DOCNO無值
                    if (supdet_seq.length > 0) {
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0118/Update_HIS14_SUPDET_DOCNO',
                            method: reqVal_p,
                            params: {
                                SUPDET_SEQ: supdet_seq
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    var show_msg = "有" + data.afrs + "筆資料存入暫存區";
                                    Ext.MessageBox.alert("提示", show_msg);
                                    msglabel("訊息區:" + show_msg);

                                    T7Load(true);
                                }
                                else if (!data.success) {
                                    Ext.MessageBox.alert('錯誤', data.msg);
                                }
                                myMask.hide();
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
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                }
            }
        }, {
            xtype: 'button',  // 添加按钮组件
            id: 'T7Tool_btn_02',
            disabled: false,
            text: '轉入請領單',
            handler: function () {
                myMask.show();
                Ext.Ajax.request({
                    url: '/api/AB0118/ins_ME_DOCD_upd_HIS14_SUPDET',
                    method: reqVal_p,
                    params: {
                        docno: T1F1
                    },
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            Ext.MessageBox.alert("提示", "轉入請領單成功");
                            msglabel('訊息區:轉入請領單成功');
                            T7Load(true);
                        }
                        else if (!data.success) {
                            Ext.MessageBox.alert('錯誤', data.msg);
                        }
                        myMask.hide();
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
                                Ext.Msg.alert('失敗', "匯入失敗");
                                break;
                        }
                    }
                });

            } // end of handel T7Tool_btn_02
        }, {
            xtype: 'button',
            id: 'T7Tool_btn_03',
            disabled: false,
            hidden: true,
            text: '關閉',
            handler: function () {
                T7Store.removeAll();
                hideWin7();
                T2Load(true);
            }
        }]
    });

    // 查詢結果資料列表
    var T7Grid = Ext.create('Ext.grid.Panel', {
        layout: 'fit',
        store: T7Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T7QueryForm]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T7Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "院內碼", dataIndex: 'MMCODE',
                width: 100
            }, {
                text: "中文品名", dataIndex: 'MMNAME_C',
                width: 150
            }, {
                text: "耗用量", dataIndex: 'USEQTY'
                , width: 80
            }, {
                text: "耗用單位", dataIndex: 'SUP_UNIT', width: 100
            }, {
                text: "病患證號", dataIndex: 'IDNO', width: 100
            }, {
                text: "病患姓名", dataIndex: 'PATIENTNAME', width: 90
            }, {
                text: "科別", dataIndex: 'SECTIONNO', width: 100
            }, {
                text: "科別名稱", dataIndex: 'SECTIONNAME', width: 100
            }, {
                text: "醫師代碼", dataIndex: 'DRID'
            }, {
                text: "醫師姓名", dataIndex: 'DRNAME', width: 90
            }, {
                text: "消耗日期", dataIndex: 'USEDATE', width: 90
            }, {
                text: "處理日期", dataIndex: 'PROCDATE', width: 90
            }, {
                text: "申請單號", dataIndex: 'DOCNO', width: 100
            }, {
                text: "流水號", dataIndex: 'SUPDET_SEQ', width: 80
            }, {
                header: "",
                flex: 1
            }
        ]
    });

    var T8Grid = Ext.create('Ext.grid.Panel', {
        layout: 'fit',
        itemId: 'T8Grid',
        store: T8Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T8QueryForm]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T8Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "科別", dataIndex: 'SECTIONNO', width: 60
            }, {
                text: "科別名稱", dataIndex: 'SECTIONNAME', width: 120
            }, {
                text: "院內碼", dataIndex: 'MMCODE', width: 80
            }, {
                text: "中文品名", dataIndex: 'MMNAME_C', width: 200
            }, {
                text: "英文品名", dataIndex: 'MMNAME_E', width: 250
            }, {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (TATabs8_Form.hidden === true) {
                        TATabs8_Form.setVisible(true);
                        TATabs8.setActiveTab('TATabs8_Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                win7.down('#TATabs7_Form_Form').expand();
                T8Rec = records.length;
                T8LastRec = records[0];
                setFormT8a();
            }
        }
    });
    function TATabs8_Form_Submit() {
        var f = TATabs8_Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set, // '/api/AB0118/Create_SEC_USEMM', // 新增/修改/刪除
                success: function (form, action) {
                    myMask.hide();
                    var f2 = TATabs8_Form.getForm(); //var f2 = T1Form.getForm(); 
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            TATabs8_Form.getForm().reset(); //T1QueryForm.getForm().reset();
                            var v = action.result.etts[0];//var v = action.result.etts[0];
                            TATabs8_Form.getForm().findField('SECTIONNO').setValue(v.SECTIONNO); //科別 T1QueryForm.getForm().findField('P0').setValue(v.DOCNO);
                            TATabs8_Form.getForm().findField('MMCODE').setValue(v.MMCODE) // 院內碼 T1QueryForm.getForm().findField('P0').setValue(v.DOCNO);
                            r.set(v); //r.set(v);
                            T8Store.insert(0, r); //T1Store.insert(0, r);
                            r.commit(); // r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "D":
                            T8Store.remove(r); //T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T8Cleanup();
                    T8Load();
                    TATabs8.setActiveTab('TATabs8_Form'); //TATabs1.setActiveTab('Query');
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
    function setFormT8(x, t) {
        win7.down('#T8Grid').mask(); // viewport.down('#t1Grid').mask();
        win7.down('#TATabs7_Form_Form').setTitle(t); // viewport.down('#form').setTitle(t + T1Name);
        win7.down('#TATabs7_Form_Form').expand(); // viewport.down('#form').expand();
        TATabs8.setActiveTab('TATabs8_Form'); // TATabs.setActiveTab('Form');
        var f = TATabs8_Form.getForm(); // var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            // 開啟可編輯
            TATabs8_Form.getForm().getFields().each(function (fc) {
                if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                    fc.setReadOnly(false);
                } else if (fc.xtype == "datefield") {
                    fc.readOnly = false;
                }
            });
            var r = Ext.create('WEBAPP.model.AB0118'); // /Scripts/app/model/ME_DOCM.js
            TATabs8_Form.loadRecord(r);; // T8Form.loadRecord(r); // 建立空白model,在新增時載入T8Form以清空欄位內容
            if (st_combo_sectionno_2.getCount() > 0) { // 科別
                //f.findField('SECTIONNO').setValue(st_combo_sectionno.getAt(0).get('VALUE'));
            }
            else {
                win7.down('#TATabs7_Form_Form').collapse();
                Ext.MessageBox.alert('錯誤', '查無 科別 資料,不得新增');
            }
            //if (st_combo_mmcode.getCount() > 0) { // 院內碼
            //    f.findField('MMCODE').setValue(st_combo_mmcode.getAt(0).get('VALUE'));
            //}
            //else {
            //    win7.down('#TATabs7_Form_Form').collapse();
            //    Ext.MessageBox.alert('錯誤', '查無 院內碼 資料,不得新增');
            //}

            f.findField('SECTIONNO').focus();
            TATabs8_Form.down('#TATabs8_Form_cancel').setVisible(true);
            TATabs8_Form.down('#TATabs8_Form_submit').setVisible(true);
        }
        f.findField('x').setValue(x);
        TATabs8_Form.down('#cancel').setVisible(true);
        TATabs8_Form.down('#submit').setVisible(true);

    }
    var TATabs8_Form = Ext.widget({
        itemID: 'TATabs8_Form',
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
                name: 'x',
                xtype: 'hidden'
            }, {
                xtype: 'combo',
                fieldLabel: '科別',
                name: 'SECTIONNO',
                store: st_combo_sectionno_2,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
            },
            T8QueryMMCode_2
            //{
            //    xtype: 'combo', // 參考 T2QueryMMCode
            //    fieldLabel: '院內碼',
            //    name: 'MMCODE',
            //    store: st_combo_mmcode,
            //    queryMode: 'local',
            //    displayField: 'COMBITEM',
            //    valueField: 'VALUE',
            //    readOnly: true,
            //    allowBlank: false, // 欄位為必填
            //    fieldCls: 'required',
            //}
        ], // end of TATabs8_Form items: 
        buttons: [
            {
                itemId: 'TATabs8_Form_submit',
                text: '儲存',
                hidden: true,
                handler: function () {
                    if (TATabs8_Form.getForm().isValid()) {  //if (this.up('TATabs8_Form').getForm().isValid()) { // 檢查TATabs8_Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                        var confirmSubmit = win7.down('#TATabs7_Form_Form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                TATabs8_Form_Submit();
                            }
                        });
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    }
                }
            }, {
                itemId: 'TATabs8_Form_cancel', text: '取消',
                handler: T8Cleanup
            }
        ] // end of buttons: 
    }); // end of var TATabs8_Form = Ext.widget({

    var TATabs8 = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [
            {
                itemId: 'TATabs8_tabpanel_add',
                title: '新增',
                border: false,
                //visible: false,
                items: [
                    TATabs8_Form
                ] // end of items TATabs8_Query
                //}, {
                //    title: '瀏覽',
                //    items: [

                //    ] // end of items TATabs8_Form
            }
        ] // end of item TATabs8 = Ext.widget('tabpanel'
    }); // end of TATabs8 = Ext.widget('tabpanel'

    var TATabs7 = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit', // 'fit',
        defaults: {
            split: true
            // layout: 'fit'
        },
        items: [{
            itemId: 'TATabs7_Query',
            title: '查詢資料',
            region: 'center',
            layout: 'border', // 讓內部有多個區域
            collapsible: false,
            border: false,
            items: [{
                region: 'center',
                layout: 'fit',
                collapsible: false,
                //title: '',
                split: true,
                items: [T7Grid]
            }] // end of items
        }, {
            itemId: 'TATabs7_Form',
            title: '設定科別耗用衛材',
            region: 'center',
            layout: 'border', // 讓內部有多個區域
            collapsible: false,
            border: false,
            items: [{
                itemId: 'TATabs7_Form_Grid',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [{
                    xtype: 'form',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    //title: '',
                    split: true,
                    items: [T8Grid]
                }] // end of TATabs7_Form_Grid items
            }, {
                itemId: 'TATabs7_Form_Form',
                region: 'east',
                collapsible: true,
                collapsed: true, // 預設縮起來
                floatable: true,
                width: 300,
                title: '',
                resizable: true,
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [
                    TATabs8 //TATabs
                ]
            }] // end of items
        }]
    });

    var win7;
    if (!win7) {
        win7 = Ext.widget('window', {
            title: '匯入HIS骨科衛材',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [TATabs7],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    } // 

    function showWin7() {
        if (win7.hidden) {
            win7.show();
        }
    }
    function hideWin7() {
        if (!win7.hidden) {
            win7.hide();
            //viewport.down('#form').collapse();
        }
    }
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });

    //showWin7();

    // -- init 預載下拉選單資料 -- 
    st_combo_sectionno.load();// 科別   st_pkdocno.load(); // 1是查詢用 有JOIN SEC_USEMM
    st_combo_sectionno_2.load();// 科別   st_pkdocno.load(); // 2是新增用的 比較寬鬆
    //st_combo_mmcode.load(); // 院內碼   st_pknote.load();

    TATabs7.setActiveTab('TATabs7_Query'); //匯入HIS骨科衛材_查詢資料

    // -- flylon end
    var win4;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '套餐',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T4Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin4() {
        if (win4.hidden) {
            T4Cleanup();
            win4.show();
        }
    }
    function hideWin4() {
        if (!win4.hidden) {
            win4.hide();
            T4Cleanup();
            //viewport.down('#form').collapse();
        }
    }

    var win5;
    if (!win5) {
        win5 = Ext.widget('window', {
            title: '低於安全存量',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T5Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin5() {
        if (win5.hidden) {
            T5Cleanup();
            T5Load();
            win5.show();
        }
    }
    function hideWin5() {
        if (!win5.hidden) {
            win5.hide();
            T5Cleanup();
            //viewport.down('#form').collapse();
        }
    }

    var win6;
    if (!win6) {
        win6 = Ext.widget('window', {
            title: '匯入',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T6Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin6() {
        if (win6.hidden) {
            T6Cleanup();
            T6Store.removeAll();
            //T6Load();
            win6.show();
        }
    }
    function hideWin6() {
        if (!win6.hidden) {
            win6.hide();
            T6Cleanup();
            //viewport.down('#form').collapse();
        }
    }
    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('P0').focus();
    T1QueryForm.getForm().findField('P2').setValue(arrP2);

    // 重算當前申請單的申請總金額
    reCalAppAmout = function () {
        if (T1LastRec) {
            Ext.Ajax.request({
                url: getDocAppAmout,
                params: {
                    docno: T1LastRec.data.DOCNO
                },
                method: reqVal_p,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        T2Query.getForm().findField('APP_AMOUT').setValue(data.msg);
                    }
                },
                failure: function (response, options) {

                }
            });
        }
    }
    function accMul(arg1, arg2) {
        var m = 0, s1 = arg1.toString(), s2 = arg2.toString();
        try {
            m += s1.split(".")[1].length;
        } catch (e) { }
        try {
            m += s2.split(".")[1].length;
        } catch (e) { }
        return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m);
    }

    function getInidChk() {
        // 若為庫房人員,需檢查APPDEPT是否與自己的INID相同,不同的申請單不能做修改/提出申請等動作
        // 若為818北投庫房人員可處理與自己的INID不同的申請單
        var user_inid = st_getlogininfo.getAt(0).get('INID');
        var is_grade1 = st_getlogininfo.getAt(0).get('IS_GRADE1');
        var hosp_code = st_getlogininfo.getAt(0).get('HOSP_CODE');
        var inidChk = false;

        if (hosp_code == "818") {
            if (is_grade1 == 'Y') {
                inidChk = true;
            } else if (is_grade1 == 'N') {
                inidChk = true;
            }
        } else {
            if (is_grade1 == 'Y' && user_inid == T1LastRec.data.APPDEPT) {
                inidChk = true;
            } else if (is_grade1 == 'N') {
                inidChk = true;
            }
        }


        return inidChk;
    }

    //#region 多筆新增
    // 供彈跳視窗內的呼叫以關閉彈跳視窗
    var callableWin = null;
    popWinForm = function () {
        var title = '';
        title = '一般物品庫備 申請單號：' + T1LastRec.data.DOCNO;

        var strUrl = "AB0118_Form?docno=" + T1LastRec.data.DOCNO + "&mStoreid=1&matClass=" + T1LastRec.data.MAT_CLASS;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                        T2Load(true);
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion
});
