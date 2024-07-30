Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var MatclassComboGet = '/api/AA0038/GetMatclassCombo';   //物料分類

    var T1Rec = 0;
    var T1LastRec = null;
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 院內碼
    var mmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    // 物品分類代碼
    var matclassQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    function setComboData() {

        Ext.Ajax.request({
            url: MatclassComboGet,
            method: reqVal_p,
            params: { p0: 'M' },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        matclassQueryStore.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            matclassQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        
    }
    setComboData();

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
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
                {
                    xtype: 'textfield',
                    store: mmcodeQueryStore,
                    id: 'P0',
                    name: 'P0',
                    enforceMaxLength: true,
                    maxLength: 13,
                    fieldLabel: '院內碼',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: matclassQueryStore,
                    id: 'P3',
                    name: 'P3',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    fieldLabel: '物料分類代碼',
                    queryMode: 'local',
                    autoSelect: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: false,
                    listConfig: { width: 180 },
                    anyMatch: true,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
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
                }, {
                    xtype: 'displayfield',
                    //labelSeparator: '',
                    value: '<span style="color:blue">次月匯入：非年度轉換使用</span>',
                    padding: '0 0 0 50'
                }
                , {
                    xtype: 'displayfield',
                    //labelSeparator: '',
                    value: '<span style="color:blue">年度匯入：年度轉換使用</span>'
                }, {
                    xtype: 'displayfield',
                    //labelSeparator: '',
                    value: '<span style="color:red">匯入檔案有填的欄位才會更新</span>'
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.MiMast_N', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
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
                    msglabel('訊息區:');
                    popWinForm('I', '');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    popWinForm('U', T1LastRec.data['MMCODE']);
                }
            },
            {
                itemId: 'import_sample', text: '下載次月範本',
                margin:'0 0 0 50',
                handler: function () {
                    msglabel('訊息區:');
                    PostForm('../../../api/AA0125/GetImportSampleExcel');
                }
            },
            {
                xtype: 'filefield',
                name: 'uploadExcel',
                id: 'uploadExcel',
                buttonText: '次月匯入',
                buttonOnly: true,
                padding: '0 0 0 0',
                width: 72,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        var file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                            msglabel('');
                        } else {
                            uploadCheckWindow.show();
                            msglabel("已選擇檔案");
                            myMask_t2.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0125/UploadCheck",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask_t2.hide();
                                    if (!data.success) {
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                                    }
                                    else {
                                        msglabel("");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';

                                        
                                        setT2Store(data.etts);
                                    }
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask_t2.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('uploadExcel').setRawValue("");
                                }
                            });
                        }
                    }

                }

            },
            {
                itemId: 'import_sampleY', text: '下載年度範本',
                margin: '0 0 0 50',
                handler: function () {
                    msglabel('訊息區:');
                    PostForm('../../../api/AA0125/GetImportSampleYExcel');
                }
            },
            {
                xtype: 'filefield',
                name: 'uploadExcelY',
                id: 'uploadExcelY',
                buttonText: '年度匯入',
                buttonOnly: true,
                padding: '0 0 0 0',
                width: 72,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        var file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                            msglabel('');
                        } else {
                            uploadCheckYWindow.show();
                            msglabel("已選擇檔案");
                            myMask_t3.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0125/UploadCheckY",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask_t3.hide();
                                    
                                    if (!data.success) {
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('uploadExcelY').fileInputEl.dom.value = '';
                                    }
                                    else {
                                        msglabel("");
                                        Ext.getCmp('uploadExcelY').fileInputEl.dom.value = '';


                                        setT3Store(data.etts);
                                    }
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask_t3.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('uploadExcel').setRawValue("");
                                }
                            });
                        }
                    }

                }

            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
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
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            renderer: function (val, meta, record) {
                if (val != null)
                    return '<a href=javascript:popWinForm("V","' + record.data['MMCODE'] + '") >' + val + '</a>';
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 170
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 170
        }, {
            text: "物料分類代碼",
            dataIndex: 'MAT_CLASS',
            width: 110
        }, {
            text: "計量單位代碼",
            dataIndex: 'BASE_UNIT',
            width: 100
        }, {
            text: "庫備識別碼",
            dataIndex: 'M_STOREID',
            width: 90
        }, {
            text: "合約識別碼",
            dataIndex: 'M_CONTID',
            width: 90
        }, {
            text: "ID碼",
            dataIndex: 'M_IDKEY',
            width: 60
        }, {
            text: "衛材料號碼",
            dataIndex: 'M_INVKEY',
            width: 80
        }, {
            text: "健保碼",
            dataIndex: 'M_NHIKEY',
            width: 100
        }, {
            text: "行政院碼",
            dataIndex: 'M_GOVKEY',
            width: 100
        }, {
            text: "長度(CM)",
            dataIndex: 'M_VOLL',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "寬度(CM)",
            dataIndex: 'M_VOLW',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "高度(CM)",
            dataIndex: 'M_VOLH',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "圓周",
            dataIndex: 'M_VOLC',
            align: 'right',
            style: 'text-align:left',
            width: 60
        }, {
            text: "材積轉換率",
            dataIndex: 'M_SWAP',
            align: 'right',
            style: 'text-align:left',
            width: 90
        }, {
            text: "是否聯標",
            dataIndex: 'M_MATID',
            width: 80
        }, {
            text: "是否供應契約",
            dataIndex: 'M_SUPPLYID',
            width: 90
        }, {
            text: "消耗屬性",
            dataIndex: 'M_CONSUMID',
            width: 80
        }, {
            text: "給付類別",
            dataIndex: 'M_PAYKIND',
            width: 80
        }, {
            text: "計費方式",
            dataIndex: 'M_PAYID',
            width: 80
        }, {
            text: "盤差種類",
            dataIndex: 'M_TRNID',
            width: 80
        }, {
            text: "申請申購識別碼",
            dataIndex: 'M_APPLYID',
            width: 100
        }, {
            text: "環保或衛署許可證",
            dataIndex: 'M_PHCTNCO',
            width: 120
        }, {
            text: "環保證號效期",
            dataIndex: 'M_ENVDT',
            width: 90
        }, {
            text: "廠商代碼",
            dataIndex: 'M_AGENNO',
            width: 80
        }, {
            text: "廠牌",
            dataIndex: 'M_AGENLAB',
            width: 80
        }, {
            text: "申購計量單位",
            dataIndex: 'M_PURUN',
            width: 90
        }, {
            text: "合約單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "折讓比",
            dataIndex: 'M_DISCPERC',
            align: 'right',
            style: 'text-align:left',
            width: 60
        }, {
            text: "批號效期註記",
            dataIndex: 'WEXP_ID',
            width: 100
        }, {
            text: "儲位記錄註記",
            dataIndex: 'WLOC_ID',
            width: 120
        }, {
            text: "是否作廢",
            dataIndex: 'CANCEL_ID',
            width: 80
        }, {
            text: "合約碼",
            dataIndex: 'CONTRACNO',
            width: 200
        }, {
            text: "最小單價",
            dataIndex: 'UPRICE',
            align: 'right',
            style: 'text-align:left',
            width: 100
        }, {
            text: "優惠合約單價",
            dataIndex: 'DISC_CPRICE',
            align: 'right',
            style: 'text-align:left',
            width: 120
        }, {
            text: "優惠最小單價",
            dataIndex: 'DISC_UPRICE',
            align: 'right',
            style: 'text-align:left',
            width: 120
        }, {
            text: "自費價",
            dataIndex: 'HOSP_PRICE',
            align: 'right',
            style: 'text-align:left',
            width: 100
        }, {
            text: "生效起日",
            dataIndex: 'BEGINDATE',
            width: 70
        }, {
            text: "生效迄日",
            dataIndex: 'ENDDATE',
            width: 70
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                T1Grid.down('#edit').setDisabled(T1Rec === 0);
            }
        }
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
                            var v = action.result.etts[0];
                            T1Query.getForm().reset();
                            T1Query.getForm().findField('P0').setValue(v.AGEN_NO);
                            T1Load();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            break;
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
                            break;
                    }
                }
            });
        }
    }

    // 供彈跳視窗內的呼叫以關閉彈跳視窗
    closeWin = function (rtnMmcode) {
        callableWin.destroy();
        callableWin = null;
        T1Query.getForm().reset();
        T1Query.getForm().findField('P0').setValue(rtnMmcode);
        T1Tool.doRefresh();
    }

    var callableWin = null;
    popWinForm = function (vType, strMmcode) {
        var strUrl = "AA0125_Form?strVtype=" + vType + "&strMmcode=" + strMmcode;
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
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, '次月衛材基本檔異動', viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }

    //#region uploadCheckWindow
    function setT2Store(datas) {
        T2Store.removeAll();
        var showConfirm = true;
        for (var i = 0; i < datas.length; i++) {
            T2Store.add(datas[i]);
            if (datas[i].SaveStatus == 'N') {
                showConfirm = false;
            }
        }

        if (showConfirm == false) {
            Ext.getCmp('uploadConfirm').hide();
        } else {
            Ext.getCmp('uploadConfirm').show();
        }
    }

    var T2Store = Ext.create('Ext.data.Store', {
        fields: ['MMCODE', 'SaveStatus', 'UploadMsg', 'MMNAME_C', 'MMNAME_E',
            'M_APPLYID', 'MIN_ORDQTY', 'M_PURUN', 'EXCH_RATIO',
            'M_CONTPRICE', 'M_STOREID', 'M_CONTID', 'BEGINDATE', 'ENDDATE',
            'M_AGENNO', 'M_PHCTNCO', 'M_ENVDT']
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        id: 't2Grid',
        columns: [{
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "是否通過",
            dataIndex: 'SaveStatus',
            width: 80,
            renderer: function (val, meta, record) {
                if (record.data.SaveStatus == 'Y') {
                    return '通過';
                }
                if (record.data.SaveStatus == 'N') {
                    return '不通過';
                }
            },
            }, {
                text: "原因",
                dataIndex: 'UploadMsg',
                width: 120,
                renderer: function (value, meta, record) {
                    meta.style = 'white-space:normal;word-break:break-all;';
                    return value;
                }
            }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150
        }, {
            text: "申請申購識別碼",
            dataIndex: 'M_APPLYID',
            width: 80
        }, {
                text: "最小撥補量",
                dataIndex: 'MIN_ORDQTY',
            width: 80
        }, {
                text: "申購計量單位",
                dataIndex: 'M_PURUN',
            width: 80
        }, {
                text: "廠商包裝轉換率",
                dataIndex: 'EXCH_RATIO',
            width: 120
        }, {
                text: "廠商代碼",
                dataIndex: 'M_AGENNO',
            width: 80
        }, {
                text: "合約單價",
                dataIndex: 'M_CONTPRICE',
            width: 80
        }, {
            text: "折讓比",
            dataIndex: 'M_DISCPERC',
            width: 80
        }, {
                text: "庫備識別碼",
                dataIndex: 'M_STOREID',
                width: 90
        }, {
                text: "合約識別碼",
                dataIndex: 'M_CONTID',
            width: 90
        }, {
            text: "環保或衛署許可證",
            dataIndex: 'M_PHCTNCO',
            width: 150
        }, {
            text: "環保證號效期",
            dataIndex: 'M_ENVDT',
            width: 150
        }, {
                text: "生效起日",
                dataIndex: 'BEGINDATE',
            width: 80
        }, {
                text: "生效迄日",
                dataIndex: 'ENDDATE',
            width: 80
        },
        {
            header: "",
            flex: 1
        }],
    });

    var uploadCheckWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'uploadCheckWindow',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        modal: true,
        title:'本次修改項目(有填欄位才會更新)',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't2Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T2Grid]
        }],
        listeners: {
            show: function () {
                uploadCheckWindow.center();
            }
        },
        buttons: [
            {
                id: 'uploadConfirm',
                hidden: true,
                text: '確定上傳',
                handler: function () {
                    
                    var temp_datas = T2Store.getData().getRange();
                    var list = [];
                    for (var i = 0; i < temp_datas.length; i++) {
                        list.push(temp_datas[i].data);
                    }

                    Ext.Ajax.request({
                        url: '/api/AA0125/UploadConfirm',
                        method: reqVal_p,
                        params: { data: Ext.util.JSON.encode(list), isYear:'N'},
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('資料上傳成功');
                                uploadCheckWindow.hide();
                            } else {
                                msglabel('資料上傳失敗');
                                Ext.Msg.alert('提醒', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            },
            {
                id: 'closeUploadCheckWindow',
                disabled: false,
                text: '關閉',
                handler: function () {
                    uploadCheckWindow.hide();
                }
            }
        ]
    });
    //#endregion

    //#region uploadCheckYWindow
    function setT3Store(datas) {
        
        T3Store.removeAll();
        var showConfirm = true;
        for (var i = 0; i < datas.length; i++) {
            T3Store.add(datas[i]);
            if (datas[i].SaveStatus == 'N') {
                showConfirm = false;
            }
        }

        if (showConfirm == false) {
            Ext.getCmp('uploadConfirmY').hide();
        } else {
            Ext.getCmp('uploadConfirmY').show();
        }
    }

    var T3Store = Ext.create('Ext.data.Store', {
        fields: ['MMCODE', 'SaveStatus', 'UploadMsg', 'MMNAME_C', 'MMNAME_E',
            'M_APPLYID', 'MIN_ORDQTY', 'M_PURUN', 'EXCH_RATIO',
            'M_AGENNO', 'M_CONTPRICE', 'DISC_CPRICE', 'UPRICE', 'DISC_UPRICE', 'M_DISCPERC',
            'M_STOREID', 'M_CONTID', 'BEGINDATE', 'ENDDATE', 
            'M_PHCTNCO', 'M_ENVDT', 'M_SUPPLYID',
            'M_VOLL', 'M_VOLW', 'M_VOLH', 'M_VOLC', 'M_SWAP'  ]
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T3',
        id: 't3Grid',
        columns: [{
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "是否通過",
            dataIndex: 'SaveStatus',
            width: 80,
            renderer: function (val, meta, record) {
                if (record.data.SaveStatus == 'Y') {
                    return '通過';
                }
                if (record.data.SaveStatus == 'N') {
                    return '不通過';
                }
            },
        }, {
            text: "原因",
            dataIndex: 'UploadMsg',
            width: 120,
            renderer: function (value, meta, record) {
                meta.style = 'white-space:normal;word-break:break-all;';
                return value;
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150
        }, {
            text: "申請申購識別碼",
            dataIndex: 'M_APPLYID',
            width: 80
        }, {
            text: "最小撥補量",
            dataIndex: 'MIN_ORDQTY',
            width: 80
        }, {
            text: "申購計量單位",
            dataIndex: 'M_PURUN',
            width: 80
        }, {
            text: "廠商包裝轉換率",
            dataIndex: 'EXCH_RATIO',
            width: 120
        }, {
            text: "廠商代碼",
            dataIndex: 'M_AGENNO',
            width: 80
        }, {
            text: "合約單價",
            dataIndex: 'M_CONTPRICE',
            width: 80
        }, {
                text: "優惠合約單價",
            dataIndex: 'DISC_CPRICE',
            width: 80
        }, {
                text: "最小單價",
                dataIndex: 'UPRICE',
            width: 80
        }, {
                text: "優惠最小單價",
                dataIndex: 'DISC_UPRICE',
            width: 80
        }, {
                text: "折讓比",
                dataIndex: 'M_DISCPERC',
            width: 80
        }, {
            text: "庫備識別碼",
            dataIndex: 'M_STOREID',
            width: 90
        }, {
            text: "合約識別碼",
            dataIndex: 'M_CONTID',
            width: 90
        }, {
                text: "是否供應契約",
            dataIndex: 'M_SUPPLYID',
            width: 90
        }, {
            text: "環保或衛署許可證",
            dataIndex: 'M_PHCTNCO',
            width: 150
        }, {
            text: "環保證號效期",
            dataIndex: 'M_ENVDT',
            width: 150
        }, {
                text: "長",
            dataIndex: 'M_VOLL',
            width: 60
        }, {
                text: "寬",
            dataIndex: 'M_VOLW',
            width: 60
        }, {
                text: "高",
            dataIndex: 'M_VOLH',
            width: 60
        }, {
                text: "圓周",
            dataIndex: 'M_VOLC',
            width: 60
        }, {
                text: "材積轉換率",
                dataIndex: 'M_SWAP',
            width: 60
        }, {
            text: "生效起日",
            dataIndex: 'BEGINDATE',
            width: 80
        }, {
            text: "生效迄日",
            dataIndex: 'ENDDATE',
            width: 80
        },
        {
            header: "",
            flex: 1
        }],
    });

    var uploadCheckYWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'uploadCheckYWindow',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        modal: true,
        title: '本次修改項目(有填欄位才會更新)',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't3Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T3Grid]
        }],
        listeners: {
            show: function () {
                uploadCheckYWindow.center();
            }
        },
        buttons: [
            {
                id: 'uploadConfirmY',
                hidden: true,
                text: '確定上傳',
                handler: function () {
                    
                    var temp_datas = T3Store.getData().getRange();
                    var list = [];
                    for (var i = 0; i < temp_datas.length; i++) {
                        list.push(temp_datas[i].data);
                    }

                    Ext.Ajax.request({
                        url: '/api/AA0125/UploadConfirm',
                        method: reqVal_p,
                        params: { data: Ext.util.JSON.encode(list), isYear: 'Y'},
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('資料上傳成功');
                                uploadCheckYWindow.hide();
                            } else {
                                msglabel('資料上傳失敗');
                                Ext.Msg.alert('提醒', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            },
            {
                id: 'closeUploadCheckYWindow',
                disabled: false,
                text: '關閉',
                handler: function () {
                    uploadCheckYWindow.hide();
                }
            }
        ]
    });
    //#endregion


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
            items: [T1Grid]
        }
        ]
    });

    var myMask_t2 = new Ext.LoadMask(Ext.getCmp('t2Grid'), { msg: '處理中...' });

    var myMask_t3 = new Ext.LoadMask(Ext.getCmp('t3Grid'), { msg: '處理中...' });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    
});
